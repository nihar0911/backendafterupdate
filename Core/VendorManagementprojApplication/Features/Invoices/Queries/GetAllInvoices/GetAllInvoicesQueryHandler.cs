using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using VendorManagementprojApplication.Contracts.Persistence;
using VendorManagementprojApplication.Contracts.Services;
using VendorManagementprojApplication.DTOs;
using VendorManagementprojDomain.Entities;

namespace VendorManagementprojApplication.Features.Invoices.Queries.GetAllInvoices;

public class GetAllInvoicesQueryHandler : IRequestHandler<GetAllInvoicesQuery, GetAllInvoicesResponse>
{
    private readonly IInvoiceRepository _invoiceRepository;
    private readonly IOutletRepository _outletRepository;
    private readonly ICurrentUserService _currentUserService;

    public GetAllInvoicesQueryHandler(
        IInvoiceRepository invoiceRepository,
        IOutletRepository outletRepository,
        ICurrentUserService currentUserService)
    {
        _invoiceRepository = invoiceRepository;
        _outletRepository = outletRepository;
        _currentUserService = currentUserService;
    }

    public async Task<GetAllInvoicesResponse> Handle(
        GetAllInvoicesQuery request,
        CancellationToken cancellationToken)
    {
        var invoices = await _invoiceRepository.GetAllAsync();

        if (_currentUserService.IsOrganizationManager)
        {
            if (_currentUserService.OrganizationID.HasValue)
            {
                var orgOutlets = await _outletRepository.GetByOrganizationIdAsync(_currentUserService.OrganizationID.Value);
                var orgOutletIds = orgOutlets.Select(o => o.OutletID).ToHashSet();
                invoices = invoices.Where(i => orgOutletIds.Contains(i.OutletID) || (i.Outlet != null && i.Outlet.OrganizationID == _currentUserService.OrganizationID.Value)).ToList();
            }
            else
            {
                invoices = new List<Invoice>();
            }
        }
        else if (_currentUserService.IsVendorManager)
        {
            if (_currentUserService.VendorID.HasValue)
            {
                invoices = invoices.Where(i => i.VendorID == _currentUserService.VendorID.Value).ToList();
            }
            else
            {
                invoices = new List<Invoice>();
            }
        }
        else if (_currentUserService.IsOutletManager || _currentUserService.IsPurchaseManager)
        {
            if (_currentUserService.OutletID.HasValue)
            {
                invoices = invoices.Where(i => i.OutletID == _currentUserService.OutletID.Value).ToList();
            }
            else
            {
                invoices = new List<Invoice>();
            }
        }

        return new GetAllInvoicesResponse
        {
            Invoices = invoices.Select(invoice => MapToDto(invoice)).ToList()
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