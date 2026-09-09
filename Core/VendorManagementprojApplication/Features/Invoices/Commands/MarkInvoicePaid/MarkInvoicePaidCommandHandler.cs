using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using VendorManagementprojApplication.Contracts.Infrastructure;
using VendorManagementprojApplication.Contracts.Persistence;
using VendorManagementprojApplication.Contracts.Services;
using VendorManagementprojApplication.DTOs;
using VendorManagementprojDomain.Entities;

namespace VendorManagementprojApplication.Features.Invoices.Commands.MarkInvoicePaid;

public class MarkInvoicePaidCommandHandler : IRequestHandler<MarkInvoicePaidCommand, MarkInvoicePaidResponse>
{
    private readonly IInvoiceRepository _invoiceRepository;
    private readonly IPaymentRepository _paymentRepository;
    private readonly IUserRepository _userRepository;
    private readonly IPurchaseOrderRepository _purchaseOrderRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly INotificationRepository _notificationRepository;
    private readonly IInvoiceDocumentService _invoiceDocumentService;

    private static readonly string[] ValidPaymentMethods = new[]
    {
        "Bank Transfer",
        "UPI",
        "Cheque",
        "Cash"
    };

    public MarkInvoicePaidCommandHandler(
        IInvoiceRepository invoiceRepository,
        IPaymentRepository paymentRepository,
        IUserRepository userRepository,
        IPurchaseOrderRepository purchaseOrderRepository,
        ICurrentUserService currentUserService,
        INotificationRepository notificationRepository,
        IInvoiceDocumentService invoiceDocumentService)
    {
        _invoiceRepository = invoiceRepository;
        _paymentRepository = paymentRepository;
        _userRepository = userRepository;
        _purchaseOrderRepository = purchaseOrderRepository;
        _currentUserService = currentUserService;
        _notificationRepository = notificationRepository;
        _invoiceDocumentService = invoiceDocumentService;
    }

    public async Task<MarkInvoicePaidResponse> Handle(
        MarkInvoicePaidCommand request,
        CancellationToken cancellationToken)
    {
        var invoice = await _invoiceRepository.GetByIdAsync(request.InvoiceID);

        if (invoice == null)
            throw new InvalidOperationException("Invoice does not exist.");

        // 1. Strictly require Approved status
        if (!string.Equals(invoice.Status, "Approved", StringComparison.OrdinalIgnoreCase))
        {
            if (string.Equals(invoice.Status, "Paid", StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException("Invoice has already been paid.");
            }

            throw new InvalidOperationException($"Only an approved invoice can be paid. Current invoice status is '{invoice.Status}'.");
        }

        // 2. Prevent duplicate payment against existing payment record
        var existingPayment = await _paymentRepository.GetByInvoiceIdAsync(invoice.InvoiceID);
        if (existingPayment != null)
        {
            throw new InvalidOperationException("A payment record already exists for this invoice.");
        }

        // 3. Validate Payment Method
        if (string.IsNullOrWhiteSpace(request.PaymentMethod) ||
            !ValidPaymentMethods.Any(m => string.Equals(m, request.PaymentMethod.Trim(), StringComparison.OrdinalIgnoreCase)))
        {
            throw new InvalidOperationException("Invalid payment method. Allowed methods are: Bank Transfer, UPI, Cheque, Cash.");
        }

        string matchedMethod = ValidPaymentMethods.First(m => string.Equals(m, request.PaymentMethod.Trim(), StringComparison.OrdinalIgnoreCase));

        // 4. Authorization & Organization Scoping
        int paidByUserId = _currentUserService.UserID ?? request.PaidByUserID;

        if (paidByUserId <= 0)
            throw new InvalidOperationException("A valid payment user is required.");

        var user = await _userRepository.GetByIdAsync(paidByUserId);
        if (user == null)
            throw new InvalidOperationException("User not found.");

        var purchaseOrder = await _purchaseOrderRepository.GetByIdAsync(invoice.PurchaseOrderID);

        if (purchaseOrder == null)
            throw new InvalidOperationException("Purchase order associated with invoice does not exist.");

        if (purchaseOrder.OutletID != invoice.OutletID)
            throw new InvalidOperationException("Invoice does not belong to the purchase order outlet.");

        if (purchaseOrder.VendorID != invoice.VendorID)
            throw new InvalidOperationException("Invoice vendor does not match the purchase order vendor.");

        bool isAuthorized = false;

        if (_currentUserService.IsAdmin || string.Equals(user.Role?.RoleName, "Admin", StringComparison.OrdinalIgnoreCase))
        {
            isAuthorized = true;
        }
        else if (_currentUserService.IsPurchaseManager || string.Equals(user.Role?.RoleName, "Purchase Manager", StringComparison.OrdinalIgnoreCase))
        {
            if (user.OutletID.HasValue && user.OutletID.Value == invoice.OutletID &&
                (!_currentUserService.OutletID.HasValue || _currentUserService.OutletID.Value == invoice.OutletID))
            {
                isAuthorized = true;
            }
        }

        if (!isAuthorized)
        {
            throw new UnauthorizedAccessException("You are not authorized to record payment for this invoice.");
        }

        // 5. Server-derived Amount directly from Invoice.TotalAmount
        var payment = new Payment
        {
            InvoiceID = invoice.InvoiceID,
            PaymentDate = request.PaymentDate ?? DateTime.Now,
            Amount = invoice.TotalAmount,
            PaymentMethod = matchedMethod,
            TransactionReference = string.IsNullOrWhiteSpace(request.TransactionReference) ? null : request.TransactionReference.Trim(),
            Status = "Paid"
        };

        await _paymentRepository.AddAsync(payment);

        invoice.Status = "Paid";

        var pdfBytes = await _invoiceDocumentService.GenerateInvoicePdfAsync(invoice);
        invoice.InvoiceDocumentBase64 = Convert.ToBase64String(pdfBytes);

        var updatedInvoice = await _invoiceRepository.UpdateAsync(invoice);

        if (updatedInvoice == null)
            throw new InvalidOperationException("Unable to update invoice status to Paid.");

        // 6. Notify Vendor Manager after successful payment
        try
        {
            var allUsers = await _userRepository.GetAllAsync();
            var vendorUsers = allUsers.Where(u => u.VendorID == invoice.VendorID).ToList();
            string orgName = purchaseOrder.Outlet?.Organization?.OrganizationName ?? "Organization";

            foreach (var vendorUser in vendorUsers)
            {
                await _notificationRepository.AddAsync(new Notification
                {
                    UserID = vendorUser.UserID,
                    Title = "Payment Received",
                    Message = $"Payment of Rs. {payment.Amount:N2} for Invoice INV-{invoice.InvoiceID} has been completed by {orgName}.",
                    NotificationType = "PaymentReceived",
                    RelatedRequestID = invoice.PurchaseOrderID,
                    RelatedVendorID = invoice.VendorID,
                    IsRead = false,
                    CreatedDate = DateTime.Now
                });
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[Payment Notification Error]: {ex.Message}");
        }

        return new MarkInvoicePaidResponse
        {
            Invoice = MapToDto(updatedInvoice)
        };
    }

    private static InvoiceDto MapToDto(Invoice invoice)
    {
        return new InvoiceDto
        {
            InvoiceID = invoice.InvoiceID,
            PurchaseOrderID = invoice.PurchaseOrderID,
            VendorID = invoice.VendorID,
            OutletID = invoice.OutletID,
            InvoiceDate = invoice.InvoiceDate,
            Subtotal = invoice.Subtotal,
            TaxAmount = invoice.TaxAmount,
            TotalAmount = invoice.TotalAmount,
            Status = invoice.Status,
            InvoiceDocumentBase64 = invoice.InvoiceDocumentBase64,
            InvoiceFileName = invoice.InvoiceFileName,
            InvoiceContentType = invoice.InvoiceContentType,
            Items = invoice.Items.Select(item => new InvoiceItemDto
            {
                InvoiceItemID = item.InvoiceItemID,
                ProductID = item.ProductID,
                Quantity = item.Quantity,
                UnitPrice = item.UnitPrice,
                TaxRate = item.TaxRate,
                Subtotal = item.Subtotal,
                TaxAmount = item.TaxAmount,
                TotalAmount = item.TotalAmount
            }).ToList()
        };
    }
}