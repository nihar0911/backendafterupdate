using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using VendorManagementprojApplication.Contracts.Persistence;
using VendorManagementprojApplication.DTOs;
using VendorManagementprojDomain.Entities;

namespace VendorManagementprojApplication.Features.PurchaseOrders.Commands.RespondToPurchaseOrder;

public class RespondToPurchaseOrderCommandHandler : IRequestHandler<RespondToPurchaseOrderCommand, RespondToPurchaseOrderResponse>
{
    private readonly IPurchaseOrderRepository _purchaseOrderRepository;
    private readonly IContractRepository _contractRepository;
    private readonly INotificationRepository _notificationRepository;
    private readonly IUserRepository _userRepository;
    private readonly IOutletRepository _outletRepository;

    public RespondToPurchaseOrderCommandHandler(
        IPurchaseOrderRepository purchaseOrderRepository,
        IContractRepository contractRepository,
        INotificationRepository notificationRepository,
        IUserRepository userRepository,
        IOutletRepository outletRepository)
    {
        _purchaseOrderRepository = purchaseOrderRepository;
        _contractRepository = contractRepository;
        _notificationRepository = notificationRepository;
        _userRepository = userRepository;
        _outletRepository = outletRepository;
    }

    public async Task<RespondToPurchaseOrderResponse> Handle(
        RespondToPurchaseOrderCommand request,
        CancellationToken cancellationToken)
    {
        var purchaseOrder = await _purchaseOrderRepository.GetByIdAsync(request.PurchaseOrderID);

        if (purchaseOrder == null)
            throw new InvalidOperationException("Purchase order does not exist.");

        if (purchaseOrder.VendorID != request.VendorID)
            throw new UnauthorizedAccessException("This purchase order does not belong to this vendor.");

        if (!string.Equals(request.Status, "Accepted", StringComparison.OrdinalIgnoreCase) &&
            !string.Equals(request.Status, "Rejected", StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("Status must be either Accepted or Rejected.");
        }

        if (!string.Equals(purchaseOrder.Status, "Pending", StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("This purchase order has already been responded to.");
        }

        if (string.Equals(request.Status, "Rejected", StringComparison.OrdinalIgnoreCase))
        {
            purchaseOrder.Status = "Rejected";

            var rejectedPurchaseOrder = await _purchaseOrderRepository.UpdateAsync(purchaseOrder);

            if (rejectedPurchaseOrder == null)
                throw new InvalidOperationException("Unable to update purchase order.");

            await NotifyStakeholdersAsync(purchaseOrder, "Purchase Order Declined", $"Vendor has declined Purchase Order PO-#{purchaseOrder.PurchaseOrderID}.", "PurchaseOrderRejected");

            return new RespondToPurchaseOrderResponse
            {
                PurchaseOrder = MapToDto(rejectedPurchaseOrder)
            };
        }

        purchaseOrder.Status = "Accepted";

        var updatedPurchaseOrder = await _purchaseOrderRepository.UpdateAsync(purchaseOrder);

        if (updatedPurchaseOrder == null)
            throw new InvalidOperationException("Unable to update purchase order.");

        await NotifyStakeholdersAsync(purchaseOrder, "Purchase Order Accepted", $"Vendor has accepted Purchase Order PO-#{purchaseOrder.PurchaseOrderID}.", "PurchaseOrderAccepted");

        return new RespondToPurchaseOrderResponse
        {
            PurchaseOrder = MapToDto(updatedPurchaseOrder)
        };
    }

    private async Task NotifyStakeholdersAsync(PurchaseOrder purchaseOrder, string title, string message, string type)
    {
        try
        {
            var allUsers = await _userRepository.GetAllAsync();

            // 1. Notify Organization Manager(s)
            var outlet = await _outletRepository.GetByIdAsync(purchaseOrder.OutletID);
            if (outlet != null)
            {
                var orgUsers = allUsers.Where(u => u.OrganizationID == outlet.OrganizationID).ToList();
                foreach (var user in orgUsers)
                {
                    await _notificationRepository.AddAsync(new Notification
                    {
                        UserID = user.UserID,
                        Title = title,
                        Message = message,
                        NotificationType = type,
                        RelatedRequestID = purchaseOrder.PurchaseOrderID,
                        RelatedVendorID = purchaseOrder.VendorID,
                        IsRead = false,
                        CreatedDate = DateTime.UtcNow
                    });
                }
            }

            // 2. Notify Outlet Manager(s)
            var outletUsers = allUsers.Where(u => u.OutletID == purchaseOrder.OutletID).ToList();
            foreach (var user in outletUsers)
            {
                await _notificationRepository.AddAsync(new Notification
                {
                    UserID = user.UserID,
                    Title = title,
                    Message = message,
                    NotificationType = type,
                    RelatedRequestID = purchaseOrder.PurchaseOrderID,
                    RelatedVendorID = purchaseOrder.VendorID,
                    IsRead = false,
                    CreatedDate = DateTime.UtcNow
                });
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[RespondToPO Notification Error]: {ex.Message}");
        }
    }

    private static PurchaseOrderDto MapToDto(PurchaseOrder purchaseOrder)
    {
        return new PurchaseOrderDto
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
            Items = purchaseOrder.Items.Select(item => new PurchaseOrderItemDto
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
            }).ToList()
        };
    }
}
