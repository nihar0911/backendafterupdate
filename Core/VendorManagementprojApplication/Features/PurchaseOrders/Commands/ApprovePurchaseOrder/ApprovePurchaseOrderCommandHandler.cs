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

namespace VendorManagementprojApplication.Features.PurchaseOrders.Commands.ApprovePurchaseOrder;

public class ApprovePurchaseOrderCommandHandler : IRequestHandler<ApprovePurchaseOrderCommand, ApprovePurchaseOrderResponse>
{
    private readonly IPurchaseOrderRepository _purchaseOrderRepository;
    private readonly IPurchaseRequestRepository _purchaseRequestRepository;
    private readonly IOutletRepository _outletRepository;
    private readonly IUserRepository _userRepository;
    private readonly INotificationRepository _notificationRepository;
    private readonly ICurrentUserService _currentUserService;

    public ApprovePurchaseOrderCommandHandler(
        IPurchaseOrderRepository purchaseOrderRepository,
        IPurchaseRequestRepository purchaseRequestRepository,
        IOutletRepository outletRepository,
        IUserRepository userRepository,
        INotificationRepository notificationRepository,
        ICurrentUserService currentUserService)
    {
        _purchaseOrderRepository = purchaseOrderRepository;
        _purchaseRequestRepository = purchaseRequestRepository;
        _outletRepository = outletRepository;
        _userRepository = userRepository;
        _notificationRepository = notificationRepository;
        _currentUserService = currentUserService;
    }

    public async Task<ApprovePurchaseOrderResponse> Handle(
        ApprovePurchaseOrderCommand request,
        CancellationToken cancellationToken)
    {
        var purchaseOrder = await _purchaseOrderRepository.GetByIdAsync(request.PurchaseOrderID);
        if (purchaseOrder == null)
            throw new InvalidOperationException("Purchase order does not exist.");

        if (!string.Equals(purchaseOrder.Status, "Awaiting Approval", StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException($"Only purchase orders awaiting approval can be approved. Current status is '{purchaseOrder.Status}'.");
        }

        // Security / Role & Organization Scoping Check
        if (!_currentUserService.IsOrganizationManager && !_currentUserService.IsAdmin)
        {
            throw new UnauthorizedAccessException("Only Organization Managers can approve purchase orders.");
        }

        if (_currentUserService.IsOrganizationManager)
        {
            if (!_currentUserService.OrganizationID.HasValue)
            {
                throw new UnauthorizedAccessException("You are not assigned to an organization.");
            }

            var outlet = await _outletRepository.GetByIdAsync(purchaseOrder.OutletID);
            if (outlet == null || outlet.OrganizationID != _currentUserService.OrganizationID.Value)
            {
                throw new UnauthorizedAccessException("You are not authorized to approve purchase orders outside your organization.");
            }
        }

        purchaseOrder.Status = "Approved";
        var updatedPurchaseOrder = await _purchaseOrderRepository.UpdateAsync(purchaseOrder);

        if (updatedPurchaseOrder == null)
            throw new InvalidOperationException("Unable to update purchase order.");

        // Notify Purchase Manager responsible for the Purchase Request / Outlet
        try
        {
            var targetUserIds = new HashSet<int>();

            var purchaseRequest = await _purchaseRequestRepository.GetByIdAsync(purchaseOrder.RequestID);
            if (purchaseRequest != null && purchaseRequest.CreatedByUserID > 0)
            {
                targetUserIds.Add(purchaseRequest.CreatedByUserID);
            }

            var pmUser = await _userRepository.GetPurchaseManagerByOutletIdAsync(purchaseOrder.OutletID);
            if (pmUser != null)
            {
                targetUserIds.Add(pmUser.UserID);
            }

            foreach (var userId in targetUserIds)
            {
                await _notificationRepository.AddAsync(new Notification
                {
                    UserID = userId,
                    Title = "Purchase Order Approved",
                    Message = $"Purchase Order PO-#{purchaseOrder.PurchaseOrderID} has been approved.",
                    NotificationType = "PurchaseOrderApproved",
                    RelatedRequestID = purchaseOrder.PurchaseOrderID,
                    RelatedVendorID = purchaseOrder.VendorID,
                    IsRead = false,
                    CreatedDate = DateTime.UtcNow
                });
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ApprovePO Notification Error]: {ex.Message}");
        }

        return new ApprovePurchaseOrderResponse
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
