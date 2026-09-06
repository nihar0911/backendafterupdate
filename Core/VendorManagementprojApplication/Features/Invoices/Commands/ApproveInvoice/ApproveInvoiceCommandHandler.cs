using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using VendorManagementprojApplication.Contracts.Persistence;
using VendorManagementprojApplication.Contracts.Services;
using VendorManagementprojApplication.DTOs;
using VendorManagementprojDomain.Entities;

namespace VendorManagementprojApplication.Features.Invoices.Commands.ApproveInvoice;

public class ApproveInvoiceCommandHandler : IRequestHandler<ApproveInvoiceCommand, ApproveInvoiceResponse>
{
    private readonly IInvoiceRepository _invoiceRepository;
    private readonly IOutletRepository _outletRepository;
    private readonly IUserRepository _userRepository;
    private readonly INotificationRepository _notificationRepository;
    private readonly ICurrentUserService _currentUserService;

    public ApproveInvoiceCommandHandler(
        IInvoiceRepository invoiceRepository,
        IOutletRepository outletRepository,
        IUserRepository userRepository,
        INotificationRepository notificationRepository,
        ICurrentUserService currentUserService)
    {
        _invoiceRepository = invoiceRepository;
        _outletRepository = outletRepository;
        _userRepository = userRepository;
        _notificationRepository = notificationRepository;
        _currentUserService = currentUserService;
    }

    public async Task<ApproveInvoiceResponse> Handle(
        ApproveInvoiceCommand request,
        CancellationToken cancellationToken)
    {
        var invoice = await _invoiceRepository.GetByIdAsync(request.InvoiceID);
        if (invoice == null)
            throw new InvalidOperationException("Invoice does not exist.");

        if (!string.Equals(invoice.Status, "Pending", StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException($"Only pending invoices can be approved. Current status is '{invoice.Status}'.");

        if (_currentUserService.IsAdmin)
        {
            // Admin retains full approval authority
        }
        else if (_currentUserService.IsPurchaseManager)
        {
            if (!_currentUserService.OutletID.HasValue)
            {
                throw new UnauthorizedAccessException("You are not authorized to approve invoices without an assigned outlet.");
            }

            if (invoice.OutletID != _currentUserService.OutletID.Value)
            {
                throw new UnauthorizedAccessException("You are not authorized to approve invoices for another outlet.");
            }
        }
        else
        {
            throw new UnauthorizedAccessException("You are not authorized to approve invoices.");
        }

        invoice.Status = "Approved";
        var updated = await _invoiceRepository.UpdateAsync(invoice);

        // Notify Vendor Manager
        try
        {
            var allUsers = await _userRepository.GetAllAsync();
            var vendorUsers = allUsers.Where(u => u.VendorID == invoice.VendorID).ToList();
            foreach (var user in vendorUsers)
            {
                await _notificationRepository.AddAsync(new Notification
                {
                    UserID = user.UserID,
                    Title = "Invoice Approved",
                    Message = $"Invoice #{invoice.InvoiceID} for PO-#{invoice.PurchaseOrderID} has been approved by the organization.",
                    NotificationType = "InvoiceApproved",
                    RelatedRequestID = invoice.PurchaseOrderID,
                    RelatedVendorID = invoice.VendorID,
                    IsRead = false,
                    CreatedDate = DateTime.Now
                });
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ApproveInvoice Notification Error]: {ex.Message}");
        }

        return new ApproveInvoiceResponse
        {
            Invoice = MapToDto(updated ?? invoice)
        };
    }

    private static InvoiceDto MapToDto(Invoice invoice)
    {
        return new InvoiceDto
        {
            InvoiceID = invoice.InvoiceID,
            PurchaseOrderID = invoice.PurchaseOrderID,
            VendorID = invoice.VendorID,
            VendorName = invoice.Vendor?.VendorName ?? string.Empty,
            OutletID = invoice.OutletID,
            OutletName = invoice.Outlet?.OutletName ?? string.Empty,
            OrganizationName = invoice.Outlet?.Organization?.OrganizationName ?? string.Empty,
            InvoiceDate = invoice.InvoiceDate,
            Subtotal = invoice.Subtotal,
            TaxAmount = invoice.TaxAmount,
            TotalAmount = invoice.TotalAmount,
            Status = invoice.Status,
            InvoiceDocumentBase64 = invoice.InvoiceDocumentBase64,
            InvoiceFileName = invoice.InvoiceFileName,
            InvoiceContentType = invoice.InvoiceContentType,
            Items = invoice.Items?.Select(item => new InvoiceItemDto
            {
                InvoiceItemID = item.InvoiceItemID,
                ProductID = item.ProductID,
                ProductName = item.Product?.ProductName ?? string.Empty,
                Quantity = item.Quantity,
                UnitPrice = item.UnitPrice,
                TaxRate = item.TaxRate,
                Subtotal = item.Subtotal,
                TaxAmount = item.TaxAmount,
                TotalAmount = item.TotalAmount
            }).ToList() ?? new System.Collections.Generic.List<InvoiceItemDto>()
        };
    }
}