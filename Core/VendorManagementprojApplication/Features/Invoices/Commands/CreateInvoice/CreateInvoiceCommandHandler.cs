using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using VendorManagementprojApplication.Contracts.Infrastructure;
using VendorManagementprojApplication.Contracts.Persistence;
using VendorManagementprojApplication.DTOs;
using VendorManagementprojDomain.Entities;

namespace VendorManagementprojApplication.Features.Invoices.Commands.CreateInvoice;

public class CreateInvoiceCommandHandler : IRequestHandler<CreateInvoiceCommand, CreateInvoiceResponse>
{
    private readonly IInvoiceRepository _invoiceRepository;
    private readonly IPurchaseOrderRepository _purchaseOrderRepository;
    private readonly IDeliveryRecordRepository _deliveryRecordRepository;
    private readonly IInvoiceDocumentService _invoiceDocumentService;
    private readonly IOutletRepository _outletRepository;
    private readonly IUserRepository _userRepository;
    private readonly INotificationRepository _notificationRepository;
    private readonly IVendorRepository _vendorRepository;

    public CreateInvoiceCommandHandler(
        IInvoiceRepository invoiceRepository,
        IPurchaseOrderRepository purchaseOrderRepository,
        IDeliveryRecordRepository deliveryRecordRepository,
        IInvoiceDocumentService invoiceDocumentService,
        IOutletRepository outletRepository,
        IUserRepository userRepository,
        INotificationRepository notificationRepository,
        IVendorRepository vendorRepository)
    {
        _invoiceRepository = invoiceRepository;
        _purchaseOrderRepository = purchaseOrderRepository;
        _deliveryRecordRepository = deliveryRecordRepository;
        _invoiceDocumentService = invoiceDocumentService;
        _outletRepository = outletRepository;
        _userRepository = userRepository;
        _notificationRepository = notificationRepository;
        _vendorRepository = vendorRepository;
    }

    public async Task<CreateInvoiceResponse> Handle(
        CreateInvoiceCommand request,
        CancellationToken cancellationToken)
    {
        var purchaseOrder = await _purchaseOrderRepository.GetByIdAsync(request.PurchaseOrderID);

        if (purchaseOrder == null)
            throw new InvalidOperationException("Purchase order does not exist.");

        if (request.VendorID <= 0)
        {
            request.VendorID = purchaseOrder.VendorID;
        }
        else if (purchaseOrder.VendorID != request.VendorID)
        {
            throw new UnauthorizedAccessException(
                "This purchase order does not belong to this vendor.");
        }

        if (!string.Equals(
            purchaseOrder.Status,
            "Delivered",
            StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException(
                "Invoice can only be created for a delivered purchase order.");

        var deliveries = await _deliveryRecordRepository
            .GetByPurchaseOrderIdAsync(request.PurchaseOrderID);

        if (deliveries == null || deliveries.Count == 0)
            throw new InvalidOperationException(
                "No delivery record exists for this purchase order.");

        if (!deliveries.Any(d =>
            string.Equals(
                d.Status,
                "Confirmed",
                StringComparison.OrdinalIgnoreCase)))
            throw new InvalidOperationException(
                "Invoice can only be created after delivery is confirmed.");

        var existingInvoice = await _invoiceRepository
            .GetByPurchaseOrderIdAsync(request.PurchaseOrderID);

        if (existingInvoice != null)
            throw new InvalidOperationException(
                "An invoice already exists for this purchase order.");

        var invoice = new Invoice
        {
            PurchaseOrderID = purchaseOrder.PurchaseOrderID,
            VendorID = purchaseOrder.VendorID,
            OutletID = purchaseOrder.OutletID,
            InvoiceDate = DateTime.Now,
            Status = "Pending",
            Items = new List<InvoiceItem>()
        };

        var confirmedDeliveries = deliveries
            .Where(d => string.Equals(d.Status, "Confirmed", StringComparison.OrdinalIgnoreCase))
            .ToList();

        foreach (var item in purchaseOrder.Items)
        {
            var deliveryRecord = confirmedDeliveries
                .FirstOrDefault(d => d.POItemID == item.POItemID || (d.PurchaseOrderItem != null && d.PurchaseOrderItem.ProductID == item.ProductID));

            decimal netQuantity = item.Quantity;
            if (deliveryRecord != null)
            {
                if (deliveryRecord.SpoiledQuantity > deliveryRecord.ReceivedQuantity)
                {
                    throw new InvalidOperationException(
                        $"Invalid delivery record for ProductID {item.ProductID}: Spoiled quantity ({deliveryRecord.SpoiledQuantity}) cannot exceed received quantity ({deliveryRecord.ReceivedQuantity}).");
                }

                netQuantity = deliveryRecord.ReceivedQuantity - deliveryRecord.SpoiledQuantity;
                if (netQuantity < 0) netQuantity = 0;
            }

            decimal subtotal = Math.Round(netQuantity * item.UnitPrice, 2);
            decimal taxAmount = Math.Round(subtotal * (item.TaxRate / 100m), 2);
            decimal totalAmount = subtotal + taxAmount;

            invoice.Items.Add(new InvoiceItem
            {
                ProductID = item.ProductID,
                Quantity = netQuantity,
                UnitPrice = item.UnitPrice,
                TaxRate = item.TaxRate,
                Subtotal = subtotal,
                TaxAmount = taxAmount,
                TotalAmount = totalAmount
            });
        }

        invoice.Subtotal = invoice.Items.Sum(i => i.Subtotal);
        invoice.TaxAmount = invoice.Items.Sum(i => i.TaxAmount);
        invoice.TotalAmount = invoice.Items.Sum(i => i.TotalAmount);

        var createdInvoice = await _invoiceRepository.AddAsync(invoice);

        var pdfBytes = await _invoiceDocumentService.GenerateInvoicePdfAsync(
            createdInvoice.InvoiceID,
            createdInvoice.PurchaseOrderID,
            createdInvoice.Subtotal,
            createdInvoice.TaxAmount,
            createdInvoice.TotalAmount);

        createdInvoice.InvoiceDocumentBase64 =
            Convert.ToBase64String(pdfBytes);

        createdInvoice.InvoiceFileName =
            $"Invoice-{createdInvoice.InvoiceID}.pdf";

        createdInvoice.InvoiceContentType =
            "application/pdf";

        await _invoiceRepository.UpdateAsync(createdInvoice);

        // Notify Organization Manager(s)
        try
        {
            var vendor = await _vendorRepository.GetByIdAsync(createdInvoice.VendorID);
            string vendorName = vendor?.VendorName ?? "Vendor";

            var outlet = await _outletRepository.GetByIdAsync(createdInvoice.OutletID);
            if (outlet != null)
            {
                var allUsers = await _userRepository.GetAllAsync();
                var orgUsers = allUsers.Where(u => u.OrganizationID == outlet.OrganizationID).ToList();
                foreach (var user in orgUsers)
                {
                    await _notificationRepository.AddAsync(new Notification
                    {
                        UserID = user.UserID,
                        Title = "Invoice Submitted",
                        Message = $"New invoice INV-{createdInvoice.InvoiceID} has been submitted by {vendorName} for PO-#{createdInvoice.PurchaseOrderID}.",
                        NotificationType = "InvoiceSubmitted",
                        RelatedRequestID = createdInvoice.PurchaseOrderID,
                        RelatedVendorID = createdInvoice.VendorID,
                        IsRead = false,
                        CreatedDate = DateTime.Now
                    });
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[CreateInvoice Notification Error]: {ex.Message}");
        }

        var fullInvoice = await _invoiceRepository.GetByIdAsync(createdInvoice.InvoiceID) ?? createdInvoice;

        return new CreateInvoiceResponse
        {
            Invoice = MapToDto(fullInvoice)
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
            }).ToList() ?? new List<InvoiceItemDto>()
        };
    }
}