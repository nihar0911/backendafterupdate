using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using VendorManagementproj.Application.Contracts.Persistence;
using VendorManagementproj.Application.Contracts.Services;
using VendorManagementproj.Application.DTOs;
using VendorManagementproj.Domain.Entities;

namespace VendorManagementproj.Application.Features.Quotations.Queries.GetQuotationById;

public class GetQuotationByIdQueryHandler : IRequestHandler<GetQuotationByIdQuery, GetQuotationByIdResponse>
{
    private readonly IQuotationRepository _quotationRepository;
    private readonly IOutletRepository _outletRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly IPurchaseOrderRepository _purchaseOrderRepository;

    public GetQuotationByIdQueryHandler(
        IQuotationRepository quotationRepository,
        IOutletRepository outletRepository,
        ICurrentUserService currentUserService,
        IPurchaseOrderRepository purchaseOrderRepository)
    {
        _quotationRepository = quotationRepository;
        _outletRepository = outletRepository;
        _currentUserService = currentUserService;
        _purchaseOrderRepository = purchaseOrderRepository;
    }

    public async Task<GetQuotationByIdResponse> Handle(GetQuotationByIdQuery request, CancellationToken cancellationToken)
    {
        var quotation = await _quotationRepository.GetByIdAsync(request.QuotationID);

        if (quotation == null)
            return new GetQuotationByIdResponse { Quotation = null };

        if (_currentUserService.IsPurchaseManager || _currentUserService.IsOutletManager)
        {
            if (!_currentUserService.OutletID.HasValue || quotation.Request == null || quotation.Request.OutletID != _currentUserService.OutletID.Value)
            {
                throw new UnauthorizedAccessException("You are not authorized to view quotations belonging to another outlet.");
            }
        }
        else if (_currentUserService.IsOrganizationManager && _currentUserService.OrganizationID.HasValue)
        {
            var orgOutlets = await _outletRepository.GetByOrganizationIdAsync(_currentUserService.OrganizationID.Value);
            var orgOutletIds = orgOutlets.Select(o => o.OutletID).ToHashSet();
            if (quotation.Request == null || !orgOutletIds.Contains(quotation.Request.OutletID))
            {
                throw new UnauthorizedAccessException("You are not authorized to view quotations outside your organization.");
            }
        }

        var existingPurchaseOrder = await _purchaseOrderRepository.GetByQuotationIdAsync(quotation.QuotationID);

        return new GetQuotationByIdResponse
        {
            Quotation = MapToDto(quotation, existingPurchaseOrder)
        };
    }

    private static QuotationDto MapToDto(Quotation quotation, PurchaseOrder? purchaseOrder = null)
    {
        return new QuotationDto
        {
            QuotationID = quotation.QuotationID,
            RequestID = quotation.RequestID,
            VendorID = quotation.VendorID,
            ValidUntil = quotation.ValidUntil,
            Status = quotation.Status,
            HasPurchaseOrder = purchaseOrder != null,
            ExistingPurchaseOrderID = purchaseOrder?.PurchaseOrderID,
            ExistingPurchaseOrderStatus = purchaseOrder?.Status,
            Items = quotation.QuotationItems.Select(item => new QuotationItemDto
            {
                QuotationItemID = item.QuotationItemID,
                ProductID = item.ProductID,
                Quantity = item.Quantity,
                UnitPrice = item.UnitPrice,
                TaxRate = item.TaxRate,
                TaxAmount = item.TaxAmount,
                TotalAmount = item.TotalAmount
            }).ToList()
        };
    }
}
