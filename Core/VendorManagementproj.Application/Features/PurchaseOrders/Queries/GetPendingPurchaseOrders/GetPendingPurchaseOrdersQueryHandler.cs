using MediatR;
using VendorManagementprojApplication.Contracts.Persistence;
using VendorManagementprojApplication.DTOs;

namespace VendorManagementprojApplication.Features.PurchaseOrders.Queries.GetPendingPurchaseOrders;

public class GetPendingPurchaseOrdersQueryHandler : IRequestHandler<GetPendingPurchaseOrdersQuery, GetPendingPurchaseOrdersResponse>
{
    private readonly IPurchaseOrderRepository _purchaseOrderRepository;

    public GetPendingPurchaseOrdersQueryHandler(
        IPurchaseOrderRepository purchaseOrderRepository)
    {
        _purchaseOrderRepository = purchaseOrderRepository;
    }

    public async Task<GetPendingPurchaseOrdersResponse> Handle(
        GetPendingPurchaseOrdersQuery request,
        CancellationToken cancellationToken)
    {
        var purchaseOrders =
            await _purchaseOrderRepository.GetPendingByVendorAsync(request.VendorID);

        return new GetPendingPurchaseOrdersResponse
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
                ApproverRole = purchaseOrder.ApproverRole,
                Items = purchaseOrder.Items.Select(item => new PurchaseOrderItemDto
                {
                    POItemID = item.POItemID,
                    ProductID = item.ProductID,
                    Quantity = item.Quantity,
                    UnitPrice = item.UnitPrice,
                    TaxRate = item.TaxRate,
                    Subtotal = item.Subtotal,
                    TaxAmount = item.TaxAmount,
                    TotalAmount = item.TotalAmount
                }).ToList()
            }).ToList()
        };
    }
}