using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using VendorManagementprojApplication.Contracts.Persistence;
using VendorManagementprojApplication.Contracts.Services;
using VendorManagementprojApplication.DTOs;
using VendorManagementprojDomain.Entities;

namespace VendorManagementprojApplication.Features.DeliveryRecords.Commands.ConfirmDeliveryRecord;

public class ConfirmDeliveryRecordCommandHandler
    : IRequestHandler<
        ConfirmDeliveryRecordCommand,
        ConfirmDeliveryRecordResponse>
{
    private readonly IDeliveryRecordRepository _deliveryRecordRepository;
    private readonly IPurchaseOrderRepository _purchaseOrderRepository;
    private readonly IUserRepository _userRepository;
    private readonly IContractRepository _contractRepository;
    private readonly INotificationRepository _notificationRepository;
    private readonly IOutletRepository _outletRepository;
    private readonly ICurrentUserService _currentUserService;

    public ConfirmDeliveryRecordCommandHandler(
        IDeliveryRecordRepository deliveryRecordRepository,
        IPurchaseOrderRepository purchaseOrderRepository,
        IUserRepository userRepository,
        IContractRepository contractRepository,
        INotificationRepository notificationRepository,
        IOutletRepository outletRepository,
        ICurrentUserService currentUserService)
    {
        _deliveryRecordRepository = deliveryRecordRepository;
        _purchaseOrderRepository = purchaseOrderRepository;
        _userRepository = userRepository;
        _contractRepository = contractRepository;
        _notificationRepository = notificationRepository;
        _outletRepository = outletRepository;
        _currentUserService = currentUserService;
    }

    public async Task<ConfirmDeliveryRecordResponse> Handle(
        ConfirmDeliveryRecordCommand request,
        CancellationToken cancellationToken)
    {
        var delivery =
            await _deliveryRecordRepository
                .GetByIdAsync(request.DeliveryRecordID);

        if (delivery == null)
            throw new InvalidOperationException(
                "Delivery record does not exist.");

        if (!string.Equals(
            delivery.Status,
            "Pending",
            StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException(
                "Only a pending delivery can be confirmed.");
        }

        var user =
            await _userRepository
                .GetByIdAsync(request.ConfirmedByUserID);

        if (user == null)
            throw new InvalidOperationException(
                "Confirming user does not exist.");

        if (!_currentUserService.IsAdmin && !_currentUserService.IsPurchaseManager)
        {
            throw new UnauthorizedAccessException(
                "Only the Purchase Manager can confirm a delivery.");
        }

        var purchaseOrder =
            await _purchaseOrderRepository
                .GetByIdAsync(delivery.PurchaseOrderID);

        if (purchaseOrder == null)
            throw new InvalidOperationException(
                "Purchase order does not exist.");

        if (_currentUserService.IsPurchaseManager)
        {
            if (!_currentUserService.OutletID.HasValue ||
                _currentUserService.OutletID.Value != purchaseOrder.OutletID)
            {
                throw new UnauthorizedAccessException(
                    "You can only confirm deliveries for your assigned outlet.");
            }
        }

        if (!string.Equals(
            purchaseOrder.Status,
            "Dispatched",
            StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException(
                "Only a dispatched purchase order can have its delivery confirmed.");
        }

        var purchaseOrderItem =
            purchaseOrder.Items
                .FirstOrDefault(item =>
                    item.POItemID == delivery.POItemID);

        if (purchaseOrderItem == null)
            throw new InvalidOperationException(
                "Purchase order item does not belong to this purchase order.");

        if (delivery.ReceivedQuantity <= 0)
            throw new InvalidOperationException(
                "Received quantity must be greater than zero.");

        if (delivery.SpoiledQuantity < 0)
            throw new InvalidOperationException(
                "Spoiled quantity cannot be negative.");

        if (delivery.SpoiledQuantity >
            delivery.ReceivedQuantity)
        {
            throw new InvalidOperationException(
                "Spoiled quantity cannot be greater than received quantity.");
        }

        var existingDeliveries =
            await _deliveryRecordRepository
                .GetByPurchaseOrderIdAsync(
                    delivery.PurchaseOrderID);

        var alreadyConfirmedQuantity =
            existingDeliveries
                .Where(x =>
                    x.POItemID == delivery.POItemID &&
                    x.DeliveryRecordID !=
                    delivery.DeliveryRecordID &&
                    string.Equals(
                        x.Status,
                        "Confirmed",
                        StringComparison.OrdinalIgnoreCase))
                .Sum(x => x.ReceivedQuantity);

        var totalConfirmedQuantity =
            alreadyConfirmedQuantity +
            delivery.ReceivedQuantity;

        if (totalConfirmedQuantity >
            purchaseOrderItem.Quantity)
        {
            throw new InvalidOperationException(
                $"Confirmed quantity cannot exceed the ordered quantity of {purchaseOrderItem.Quantity}.");
        }

        // Check for active contract allocation if one exists for this order
        var contracts =
            await _contractRepository.GetAllAsync();

        var contract =
            contracts.FirstOrDefault(c =>
                c.OutletID == purchaseOrder.OutletID &&
                c.ProductID == purchaseOrderItem.ProductID &&
                string.Equals(
                    c.Status,
                    "Active",
                    StringComparison.OrdinalIgnoreCase) &&
                c.StartDate <= DateTime.Now &&
                c.EndDate >= DateTime.Now &&
                c.VendorAllocations.Any(a =>
                    a.VendorID == purchaseOrder.VendorID &&
                    string.Equals(
                        a.Status,
                        "Active",
                        StringComparison.OrdinalIgnoreCase)));

        ContractVendorAllocation? allocation = null;
        if (contract != null)
        {
            allocation =
                contract.VendorAllocations.FirstOrDefault(a =>
                    a.VendorID == purchaseOrder.VendorID &&
                    string.Equals(
                        a.Status,
                        "Active",
                        StringComparison.OrdinalIgnoreCase));
        }

        delivery.Status = "Confirmed";

        delivery.ConfirmedByUserID =
            request.ConfirmedByUserID;

        delivery.ConfirmedAt =
            DateTime.Now;

        await _deliveryRecordRepository
            .UpdateAsync(delivery);

        // If a contract exists and has an active allocation, track contract usage (capped at remaining allocation)
        if (contract != null && allocation != null)
        {
            decimal remainingContractQuantity = allocation.AllocatedQuantity - allocation.UsedQuantity;
            decimal netReceived = delivery.ReceivedQuantity - delivery.SpoiledQuantity;
            if (netReceived < 0) netReceived = 0;

            decimal contractConsumable = Math.Min(netReceived, Math.Max(0, remainingContractQuantity));
            allocation.UsedQuantity += contractConsumable;

            if (allocation.UsedQuantity >=
                allocation.AllocatedQuantity)
            {
                allocation.UsedQuantity =
                    allocation.AllocatedQuantity;

                allocation.Status = "Reached";
            }

            contract.UsedQuantity =
                contract.VendorAllocations
                    .Sum(a => a.UsedQuantity);

            var allAllocationsReached =
                contract.VendorAllocations.All(a =>
                    string.Equals(
                        a.Status,
                        "Reached",
                        StringComparison.OrdinalIgnoreCase));

            if (allAllocationsReached)
                contract.Status = "Reached";

            await _contractRepository
                .UpdateAsync(contract);
        }

        var allItemsFullyReceived = true;

        foreach (var item in purchaseOrder.Items)
        {
            var confirmedQuantity =
                existingDeliveries
                    .Where(x =>
                        x.POItemID == item.POItemID &&
                        x.DeliveryRecordID !=
                        delivery.DeliveryRecordID &&
                        string.Equals(
                            x.Status,
                            "Confirmed",
                            StringComparison.OrdinalIgnoreCase))
                    .Sum(x => x.ReceivedQuantity);

            confirmedQuantity +=
                delivery.POItemID == item.POItemID
                    ? delivery.ReceivedQuantity
                    : 0;

            if (confirmedQuantity < item.Quantity)
            {
                allItemsFullyReceived = false;
                break;
            }
        }

        if (allItemsFullyReceived)
        {
            purchaseOrder.Status = "Delivered";
            purchaseOrder.ActualDeliveryDate = delivery.DeliveryDate;

            if (purchaseOrder.ExpectedDeliveryDate.HasValue)
            {
                int rawDelayDays = (delivery.DeliveryDate.Date - purchaseOrder.ExpectedDeliveryDate.Value.Date).Days;
                if (rawDelayDays > 0)
                {
                    purchaseOrder.DeliveryStatus = $"Delayed by {rawDelayDays} day{(rawDelayDays > 1 ? "s" : "")}";
                }
                else
                {
                    purchaseOrder.DeliveryStatus = "On-Time";
                }
            }
            else
            {
                purchaseOrder.DeliveryStatus = "Delivered";
            }

            await _purchaseOrderRepository
                .UpdateAsync(purchaseOrder);

            // Send notification to Organization Manager(s)
            try
            {
                var outlet = await _outletRepository.GetByIdAsync(purchaseOrder.OutletID);
                if (outlet != null)
                {
                    var allUsers = await _userRepository.GetAllAsync();
                    var orgManagers = allUsers.Where(u => u.OrganizationID == outlet.OrganizationID).ToList();
                    foreach (var orgUser in orgManagers)
                    {
                        await _notificationRepository.AddAsync(new Notification
                        {
                            UserID = orgUser.UserID,
                            Title = "Purchase Order Delivered",
                            Message = $"Purchase Order PO-#{purchaseOrder.PurchaseOrderID} has been delivered and confirmed at {outlet.OutletName}.",
                            NotificationType = "PurchaseOrderDelivered",
                            RelatedRequestID = purchaseOrder.PurchaseOrderID,
                            RelatedVendorID = purchaseOrder.VendorID,
                            IsRead = false,
                            CreatedDate = DateTime.Now
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ConfirmDelivery Notification Error]: {ex.Message}");
            }
        }

        return new ConfirmDeliveryRecordResponse
        {
            DeliveryRecord =
                MapToDto(delivery)
        };
    }

    private static DeliveryRecordDto MapToDto(
        VendorManagementprojDomain.Entities.DeliveryRecord delivery)
    {
        return new DeliveryRecordDto
        {
            DeliveryRecordID =
                delivery.DeliveryRecordID,

            PurchaseOrderID =
                delivery.PurchaseOrderID,

            POItemID =
                delivery.POItemID,

            DeliveryDate =
                delivery.DeliveryDate,

            OrderedQuantity =
                delivery.OrderedQuantity,

            ReceivedQuantity =
                delivery.ReceivedQuantity,

            SpoiledQuantity =
                delivery.SpoiledQuantity,

            SpoilagePercentage =
                delivery.SpoilagePercentage,

            Status =
                delivery.Status,

            ConfirmedByUserID =
                delivery.ConfirmedByUserID,

            ConfirmedAt =
                delivery.ConfirmedAt
        };
    }
}
