using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using VendorManagementprojApplication.Contracts.Persistence;
using VendorManagementprojApplication.Contracts.Services;
using VendorManagementprojApplication.DTOs;

namespace VendorManagementprojApplication.Features.PurchaseOrders.Queries.GetPurchaseOrderById;

public class GetPurchaseOrderByIdQueryHandler : IRequestHandler<GetPurchaseOrderByIdQuery, GetPurchaseOrderByIdResponse>
{
    private readonly IPurchaseOrderRepository _purchaseOrderRepository;
    private readonly IOutletRepository _outletRepository;
    private readonly ICurrentUserService _currentUserService;

    public GetPurchaseOrderByIdQueryHandler(
        IPurchaseOrderRepository purchaseOrderRepository,
        IOutletRepository outletRepository,
        ICurrentUserService currentUserService)
    {
        _purchaseOrderRepository = purchaseOrderRepository;
        _outletRepository = outletRepository;
        _currentUserService = currentUserService;
    }

    public async Task<GetPurchaseOrderByIdResponse> Handle(
        GetPurchaseOrderByIdQuery request,
        CancellationToken cancellationToken)
    {
        var purchaseOrder = await _purchaseOrderRepository.GetByIdAsync(
            request.PurchaseOrderID);

        if (purchaseOrder == null)
            return new GetPurchaseOrderByIdResponse { PurchaseOrder = null };

        // Security / Role Authorization Check
        if ((_currentUserService.IsPurchaseManager || _currentUserService.IsOutletManager) && _currentUserService.OutletID.HasValue)
        {
            if (purchaseOrder.OutletID != _currentUserService.OutletID.Value)
            {
                throw new UnauthorizedAccessException("You are not authorized to view purchase orders belonging to another outlet.");
            }
        }
        else if (_currentUserService.IsPurchaseManager && !_currentUserService.OutletID.HasValue)
        {
            throw new UnauthorizedAccessException("You are not authorized to view purchase orders without an assigned outlet.");
        }
        else if (_currentUserService.IsVendorManager && _currentUserService.VendorID.HasValue)
        {
            if (purchaseOrder.VendorID != _currentUserService.VendorID.Value)
            {
                throw new UnauthorizedAccessException("You are not authorized to view purchase orders belonging to another vendor.");
            }
        }
        else if (_currentUserService.IsOrganizationManager && _currentUserService.OrganizationID.HasValue)
        {
            var orgOutlets = await _outletRepository.GetByOrganizationIdAsync(_currentUserService.OrganizationID.Value);
            var orgOutletIds = orgOutlets.Select(o => o.OutletID).ToHashSet();
            if (!orgOutletIds.Contains(purchaseOrder.OutletID))
            {
                throw new UnauthorizedAccessException("You are not authorized to view purchase orders outside your organization.");
            }
        }

        return new GetPurchaseOrderByIdResponse
        {
            PurchaseOrder = new PurchaseOrderDto
            {
                PurchaseOrderID = purchaseOrder.PurchaseOrderID,
                RequestID = purchaseOrder.RequestID,
                VendorID = purchaseOrder.VendorID,
                QuotationID = purchaseOrder.QuotationID,
                OutletID = purchaseOrder.OutletID,
                OrderDate = purchaseOrder.OrderDate,
                ExpectedDeliveryDate = purchaseOrder.ExpectedDeliveryDate,
                DispatchDateTime = purchaseOrder.DispatchDateTime,
                ActualDeliveryDate = purchaseOrder.ActualDeliveryDate,
                DeliveryStatus = purchaseOrder.DeliveryStatus,
                Status = purchaseOrder.Status,
                Items = purchaseOrder.Items?.Select(item => new PurchaseOrderItemDto
                {
                    POItemID = item.POItemID,
                    ProductID = item.ProductID,
                    Quantity = item.Quantity,
                    UnitPrice = item.UnitPrice,
                    DiscountAmount = item.DiscountAmount,
                    TaxRate = item.TaxRate,
                    Subtotal = item.Subtotal,
                    TaxAmount = item.TaxAmount,
                    TotalAmount = item.TotalAmount
                }).ToList() ?? new List<PurchaseOrderItemDto>()
            }
        };
    }
}
