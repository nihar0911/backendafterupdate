using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using VendorManagementprojApplication.Contracts.Persistence;
using VendorManagementprojApplication.DTOs;
using VendorManagementprojDomain.Entities;

namespace VendorManagementprojApplication.Features.DeliveryRecords.Commands.CreateDeliveryRecord;

public class CreateDeliveryRecordCommandHandler : IRequestHandler<CreateDeliveryRecordCommand, CreateDeliveryRecordResponse>
{
    private readonly IDeliveryRecordRepository _deliveryRecordRepository;
    private readonly IPurchaseOrderRepository _purchaseOrderRepository;

    public CreateDeliveryRecordCommandHandler(
        IDeliveryRecordRepository deliveryRecordRepository,
        IPurchaseOrderRepository purchaseOrderRepository)
    {
        _deliveryRecordRepository = deliveryRecordRepository;
        _purchaseOrderRepository = purchaseOrderRepository;
    }

    public async Task<CreateDeliveryRecordResponse> Handle(
        CreateDeliveryRecordCommand request,
        CancellationToken cancellationToken)
    {
        if (request.ReceivedQuantity <= 0)
            throw new InvalidOperationException(
                "Received quantity must be greater than zero.");

        if (request.SpoiledQuantity < 0)
            throw new InvalidOperationException(
                "Spoiled quantity cannot be negative.");

        if (request.SpoiledQuantity > request.ReceivedQuantity)
            throw new InvalidOperationException(
                "Spoiled quantity cannot be greater than received quantity.");

        var purchaseOrder =
            await _purchaseOrderRepository.GetByIdAsync(
                request.PurchaseOrderID);

        if (purchaseOrder == null)
            throw new InvalidOperationException(
                "Purchase order does not exist.");

        if (!string.Equals(
            purchaseOrder.Status,
            "Dispatched",
            StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException(
                "Delivery can only be created for a dispatched purchase order.");

        var purchaseOrderItem =
            purchaseOrder.Items
                .FirstOrDefault(x =>
                    x.POItemID == request.POItemID);

        if (purchaseOrderItem == null)
            throw new InvalidOperationException(
                "Purchase order item does not belong to this purchase order.");

        var existingDeliveries =
            await _deliveryRecordRepository
                .GetByPurchaseOrderIdAsync(
                    request.PurchaseOrderID);

        // Only Confirmed deliveries count toward the already received total
        var alreadyReceived =
            existingDeliveries
                .Where(x =>
                    x.POItemID == request.POItemID &&
                    string.Equals(
                        x.Status,
                        "Confirmed",
                        StringComparison.OrdinalIgnoreCase))
                .Sum(x => x.ReceivedQuantity);

        var remainingQuantity =
            purchaseOrderItem.Quantity - alreadyReceived;

        if (remainingQuantity <= 0)
            throw new InvalidOperationException(
                "The ordered quantity for this purchase order item has already been fully received.");

        if (request.ReceivedQuantity > remainingQuantity)
            throw new InvalidOperationException(
                $"Received quantity cannot exceed the remaining quantity of {remainingQuantity}.");

        var spoilagePercentage =
            request.SpoiledQuantity /
            request.ReceivedQuantity * 100;

        DateTime deliveryDate = request.DeliveryDate ?? DateTime.Now;

        // If an unconfirmed Pending delivery record already exists for this PO and item, reuse/update it
        var existingPending = existingDeliveries
            .FirstOrDefault(x =>
                x.POItemID == request.POItemID &&
                string.Equals(x.Status, "Pending", StringComparison.OrdinalIgnoreCase));

        if (existingPending != null)
        {
            existingPending.OrderedQuantity = purchaseOrderItem.Quantity;
            existingPending.ReceivedQuantity = request.ReceivedQuantity;
            existingPending.SpoiledQuantity = request.SpoiledQuantity;
            existingPending.SpoilagePercentage = spoilagePercentage;
            existingPending.DeliveryDate = deliveryDate;

            await _deliveryRecordRepository.UpdateAsync(existingPending);

            return new CreateDeliveryRecordResponse
            {
                DeliveryRecord = MapToDto(existingPending)
            };
        }

        var deliveryRecord = new DeliveryRecord
        {
            PurchaseOrderID =
                request.PurchaseOrderID,

            POItemID =
                request.POItemID,

            DeliveryDate =
                deliveryDate,

            OrderedQuantity =
                purchaseOrderItem.Quantity,

            ReceivedQuantity =
                request.ReceivedQuantity,

            SpoiledQuantity =
                request.SpoiledQuantity,

            SpoilagePercentage =
                spoilagePercentage,

            Status =
                "Pending",

            ConfirmedByUserID =
                null,

            ConfirmedAt =
                null
        };

        var createdDelivery =
            await _deliveryRecordRepository
                .AddAsync(deliveryRecord);

        return new CreateDeliveryRecordResponse
        {
            DeliveryRecord = MapToDto(createdDelivery)
        };
    }

    private static DeliveryRecordDto MapToDto(
        DeliveryRecord deliveryRecord)
    {
        return new DeliveryRecordDto
        {
            DeliveryRecordID =
                deliveryRecord.DeliveryRecordID,

            PurchaseOrderID =
                deliveryRecord.PurchaseOrderID,

            POItemID =
                deliveryRecord.POItemID,

            DeliveryDate =
                deliveryRecord.DeliveryDate,

            OrderedQuantity =
                deliveryRecord.OrderedQuantity,

            ReceivedQuantity =
                deliveryRecord.ReceivedQuantity,

            SpoiledQuantity =
                deliveryRecord.SpoiledQuantity,

            SpoilagePercentage =
                deliveryRecord.SpoilagePercentage,

            Status =
                deliveryRecord.Status,

            ConfirmedByUserID =
                deliveryRecord.ConfirmedByUserID,

            ConfirmedAt =
                deliveryRecord.ConfirmedAt
        };
    }

    private static DateTime DeliveryDate(DeliveryRecord deliveryRecord) => deliveryRecord.DeliveryDate;
}
