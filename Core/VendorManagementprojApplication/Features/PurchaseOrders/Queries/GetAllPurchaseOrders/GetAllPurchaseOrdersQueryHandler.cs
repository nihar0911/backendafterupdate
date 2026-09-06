using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using VendorManagementprojApplication.Contracts.Persistence;
using VendorManagementprojApplication.Contracts.Services;
using VendorManagementprojApplication.DTOs;
using VendorManagementprojDomain.Entities;

namespace VendorManagementprojApplication.Features.PurchaseOrders.Queries.GetAllPurchaseOrders;

public class GetAllPurchaseOrdersQueryHandler : IRequestHandler<GetAllPurchaseOrdersQuery, GetAllPurchaseOrdersResponse>
{
    private readonly IPurchaseOrderRepository _purchaseOrderRepository;
    private readonly IOutletRepository _outletRepository;
    private readonly ICurrentUserService _currentUserService;

    public GetAllPurchaseOrdersQueryHandler(
        IPurchaseOrderRepository purchaseOrderRepository,
        IOutletRepository outletRepository,
        ICurrentUserService currentUserService)
    {
        _purchaseOrderRepository = purchaseOrderRepository;
        _outletRepository = outletRepository;
        _currentUserService = currentUserService;
    }

    public async Task<GetAllPurchaseOrdersResponse> Handle(
        GetAllPurchaseOrdersQuery request,
        CancellationToken cancellationToken)
    {
        var purchaseOrders = await _purchaseOrderRepository.GetAllAsync();

        if (_currentUserService.IsOrganizationManager)
        {
            if (_currentUserService.OrganizationID.HasValue)
            {
                var orgOutlets = await _outletRepository.GetByOrganizationIdAsync(_currentUserService.OrganizationID.Value);
                var orgOutletIds = orgOutlets.Select(o => o.OutletID).ToHashSet();
                purchaseOrders = purchaseOrders.Where(po => orgOutletIds.Contains(po.OutletID) || (po.Outlet != null && po.Outlet.OrganizationID == _currentUserService.OrganizationID.Value)).ToList();
            }
            else
            {
                purchaseOrders = new List<PurchaseOrder>();
            }
        }
        else if (_currentUserService.IsPurchaseManager || _currentUserService.IsOutletManager)
        {
            if (_currentUserService.OutletID.HasValue)
            {
                purchaseOrders = purchaseOrders.Where(po => po.OutletID == _currentUserService.OutletID.Value).ToList();
            }
            else
            {
                purchaseOrders = new List<PurchaseOrder>();
            }
        }
        else if (_currentUserService.IsVendorManager)
        {
            if (_currentUserService.VendorID.HasValue)
            {
                purchaseOrders = purchaseOrders
                    .Where(po => po.VendorID == _currentUserService.VendorID.Value &&
                                 po.Status != "Awaiting Approval" &&
                                 po.Status != "Approved")
                    .ToList();
            }
            else
            {
                purchaseOrders = new List<PurchaseOrder>();
            }
        }

        return new GetAllPurchaseOrdersResponse
        {
            PurchaseOrders = purchaseOrders.Select(purchaseOrder => new PurchaseOrderDto
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
            }).ToList()
        };
    }
}
