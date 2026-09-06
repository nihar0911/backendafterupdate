using MediatR;
using VendorManagementprojApplication.Contracts.Persistence;
using VendorManagementprojApplication.DTOs;

namespace VendorManagementprojApplication.Features.DeliveryRecords.Queries.GetDeliveriesByPurchaseOrder;

public class GBDPOhandler
    : IRequestHandler<
        GetDeliveriesByPurchaseOrderQuery,
        GetDeliveriesByPurchaseOrderResponse>
{
    private readonly IDeliveryRecordRepository _deliveryRecordRepository;
    private readonly IPurchaseOrderRepository _purchaseOrderRepository;

    public GBDPOhandler(
        IDeliveryRecordRepository deliveryRecordRepository,
        IPurchaseOrderRepository purchaseOrderRepository)
    {
        _deliveryRecordRepository = deliveryRecordRepository;
        _purchaseOrderRepository = purchaseOrderRepository;
    }

    public async Task<GetDeliveriesByPurchaseOrderResponse> Handle(
        GetDeliveriesByPurchaseOrderQuery request,
        CancellationToken cancellationToken)
    {
        var purchaseOrder =
            await _purchaseOrderRepository
                .GetByIdAsync(request.PurchaseOrderID);

        if (purchaseOrder == null)
            throw new InvalidOperationException(
                "Purchase order does not exist.");

        var deliveries =
            await _deliveryRecordRepository
                .GetByPurchaseOrderIdAsync(
                    request.PurchaseOrderID);

        return new GetDeliveriesByPurchaseOrderResponse
        {
            Deliveries = deliveries.Select(d => new DeliveryRecordDto
            {
                DeliveryRecordID = d.DeliveryRecordID,
                PurchaseOrderID = d.PurchaseOrderID,
                POItemID = d.POItemID,
                DeliveryDate = d.DeliveryDate,
                OrderedQuantity = d.OrderedQuantity,
                ReceivedQuantity = d.ReceivedQuantity,
                SpoiledQuantity = d.SpoiledQuantity,
                SpoilagePercentage = d.SpoilagePercentage,
                Status = d.Status,
                ConfirmedByUserID = d.ConfirmedByUserID,
                ConfirmedAt = d.ConfirmedAt
            }).ToList()
        };
    }
}