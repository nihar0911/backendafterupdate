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

namespace VendorManagementprojApplication.Features.PurchaseOrders.Commands.SendPurchaseOrder;

public class SendPurchaseOrderCommandHandler : IRequestHandler<SendPurchaseOrderCommand, SendPurchaseOrderResponse>
{
    private readonly IPurchaseOrderRepository _purchaseOrderRepository;
    private readonly IUserRepository _userRepository;
    private readonly INotificationRepository _notificationRepository;
    private readonly ICurrentUserService _currentUserService;

    public SendPurchaseOrderCommandHandler(
        IPurchaseOrderRepository purchaseOrderRepository,
        IUserRepository userRepository,
        INotificationRepository notificationRepository,
        ICurrentUserService currentUserService)
    {
        _purchaseOrderRepository = purchaseOrderRepository;
        _userRepository = userRepository;
        _notificationRepository = notificationRepository;
        _currentUserService = currentUserService;
    }

    public async Task<SendPurchaseOrderResponse> Handle(
        SendPurchaseOrderCommand request,
        CancellationToken cancellationToken)
    {
        var purchaseOrder = await _purchaseOrderRepository.GetByIdAsync(request.PurchaseOrderID);
        if (purchaseOrder == null)
            throw new InvalidOperationException("Purchase order does not exist.");

        // Security / Role & Outlet Authorization Check
        if (!_currentUserService.IsPurchaseManager && !_currentUserService.IsAdmin)
        {
            throw new UnauthorizedAccessException("Only Purchase Managers can send purchase orders to vendors.");
        }

        if (_currentUserService.IsPurchaseManager)
        {
            if (!_currentUserService.OutletID.HasValue)
            {
                throw new UnauthorizedAccessException("You are not assigned to an outlet.");
            }

            if (purchaseOrder.OutletID != _currentUserService.OutletID.Value)
            {
                throw new UnauthorizedAccessException("You are not authorized to send purchase orders belonging to another outlet.");
            }
        }

        // Status Transition Validation: Only "Approved" -> "Pending"
        if (!string.Equals(purchaseOrder.Status, "Approved", StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException($"Only approved purchase orders can be sent to vendors. Current status is '{purchaseOrder.Status}'.");
        }

        purchaseOrder.Status = "Pending";
        var updatedPurchaseOrder = await _purchaseOrderRepository.UpdateAsync(purchaseOrder);

        if (updatedPurchaseOrder == null)
            throw new InvalidOperationException("Unable to update purchase order.");

        // Notify Vendor Manager(s) that the PO has been sent
        try
        {
            var allUsers = await _userRepository.GetAllAsync();
            var vendorUsers = allUsers.Where(u => u.VendorID == purchaseOrder.VendorID).ToList();

            foreach (var user in vendorUsers)
            {
                await _notificationRepository.AddAsync(new Notification
                {
                    UserID = user.UserID,
                    Title = "New Purchase Order",
                    Message = $"Purchase Order PO-#{purchaseOrder.PurchaseOrderID} has been sent to you.",
                    NotificationType = "PurchaseOrderSent",
                    RelatedRequestID = purchaseOrder.PurchaseOrderID,
                    RelatedVendorID = purchaseOrder.VendorID,
                    IsRead = false,
                    CreatedDate = DateTime.UtcNow
                });
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[SendPO Notification Error]: {ex.Message}");
        }

        return new SendPurchaseOrderResponse
        {
            PurchaseOrder = MapToDto(updatedPurchaseOrder)
        };
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
            ApproverRole = purchaseOrder.ApproverRole,
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
        };
    }
}
