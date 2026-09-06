using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using VendorManagementprojApplication.Contracts.Persistence;
using VendorManagementprojApplication.Contracts.Services;
using VendorManagementprojApplication.DTOs;
using VendorManagementprojDomain.Entities;

namespace VendorManagementprojApplication.Features.Invoices.Queries.GetInvoiceById;

public class GetInvoiceByIdQueryHandler : IRequestHandler<GetInvoiceByIdQuery, GetInvoiceByIdResponse>
{
    private readonly IInvoiceRepository _invoiceRepository;
    private readonly IOutletRepository _outletRepository;
    private readonly ICurrentUserService _currentUserService;

    public GetInvoiceByIdQueryHandler(
        IInvoiceRepository invoiceRepository,
        IOutletRepository outletRepository,
        ICurrentUserService currentUserService)
    {
        _invoiceRepository = invoiceRepository;
        _outletRepository = outletRepository;
        _currentUserService = currentUserService;
    }

    public async Task<GetInvoiceByIdResponse> Handle(
        GetInvoiceByIdQuery request,
        CancellationToken cancellationToken)
    {
        var invoice = await _invoiceRepository.GetByIdAsync(request.InvoiceID);

        if (invoice == null)
            throw new InvalidOperationException("Invoice does not exist.");

        // Role authorization check
        if (_currentUserService.IsVendorManager && _currentUserService.VendorID.HasValue)
        {
            if (invoice.VendorID != _currentUserService.VendorID.Value)
            {
                throw new UnauthorizedAccessException("You are not authorized to view this invoice.");
            }
        }
        else if ((_currentUserService.IsOutletManager || _currentUserService.IsPurchaseManager) && _currentUserService.OutletID.HasValue)
        {
            if (invoice.OutletID != _currentUserService.OutletID.Value)
            {
                throw new UnauthorizedAccessException("You are not authorized to view this invoice.");
            }
        }
        else if ((_currentUserService.IsOutletManager || _currentUserService.IsPurchaseManager) && !_currentUserService.OutletID.HasValue)
        {
            throw new UnauthorizedAccessException("You are not authorized to view invoices without an assigned outlet.");
        }
        else if (_currentUserService.IsOrganizationManager && _currentUserService.OrganizationID.HasValue)
        {
            var outlet = await _outletRepository.GetByIdAsync(invoice.OutletID);
            if (outlet == null || outlet.OrganizationID != _currentUserService.OrganizationID.Value)
            {
                throw new UnauthorizedAccessException("You are not authorized to view invoices outside your organization.");
            }
        }

        return new GetInvoiceByIdResponse
        {
            Invoice = MapToDto(invoice)
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