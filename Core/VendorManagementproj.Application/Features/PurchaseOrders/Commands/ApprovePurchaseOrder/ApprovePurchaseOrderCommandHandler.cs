using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using VendorManagementproj.Application.Common;
using VendorManagementproj.Application.Contracts.Persistence;
using VendorManagementproj.Application.Contracts.Services;
using VendorManagementproj.Application.DTOs;
using VendorManagementproj.Domain.Entities;

namespace VendorManagementproj.Application.Features.PurchaseOrders.Commands.ApprovePurchaseOrder;

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

        var outlet = await _outletRepository.GetByIdAsync(purchaseOrder.OutletID);
        PurchaseOrderApprover.EnsureCurrentUserCanDecide(_currentUserService, purchaseOrder, outlet);

        purchaseOrder.Status = "Pending";
        var updatedPurchaseOrder = await _purchaseOrderRepository.UpdateAsync(purchaseOrder);

        if (updatedPurchaseOrder == null)
            throw new InvalidOperationException("Unable to update purchase order.");

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

            var poProducts = purchaseOrder.Items?.Select(poi =>
            {
                var prItem = purchaseRequest?.Items?.FirstOrDefault(pi => pi.ProductID == poi.ProductID);
                var name = poi.Product?.ProductName ?? prItem?.Product?.ProductName ?? $"Product #{poi.ProductID}";
                var unit = poi.Product?.Unit ?? prItem?.Unit ?? prItem?.Product?.Unit;
                return ((string?)name, poi.Quantity, (string?)unit);
            });
            var (prodTitle, prodMsg) = NotificationProductFormatter.FormatProductSummaries(poProducts);
            string vendorName = purchaseOrder.Vendor?.VendorName ?? "the vendor";

            string pmTitle = string.IsNullOrWhiteSpace(prodTitle) ? "Purchase Order Approved" : $"Purchase Order Approved: {prodTitle}";
            string pmMsg = string.IsNullOrWhiteSpace(prodMsg)
                ? $"Purchase Order PO-{purchaseOrder.PurchaseOrderID} was approved and placed with {vendorName}."
                : $"Purchase Order PO-{purchaseOrder.PurchaseOrderID} for {prodMsg} was approved and placed with {vendorName}.";

            string vendorTitle = string.IsNullOrWhiteSpace(prodTitle) ? "New Purchase Order" : $"New Purchase Order: {prodTitle}";
            string vendorMsg = string.IsNullOrWhiteSpace(prodMsg)
                ? $"Purchase Order PO-{purchaseOrder.PurchaseOrderID} has been placed with you."
                : $"Purchase Order PO-{purchaseOrder.PurchaseOrderID} for {prodMsg} has been placed with you.";

            foreach (var userId in targetUserIds)
            {
                await _notificationRepository.AddAsync(new Notification
                {
                    UserID = userId,
                    Title = pmTitle,
                    Message = pmMsg,
                    NotificationType = "PurchaseOrderApproved",
                    RelatedRequestID = purchaseOrder.PurchaseOrderID,
                    RelatedVendorID = purchaseOrder.VendorID,
                    IsRead = false,
                    CreatedDate = DateTime.Now
                });
            }

            var allUsers = await _userRepository.GetAllAsync();
            var vendorUsers = allUsers.Where(u => u.VendorID == purchaseOrder.VendorID).ToList();
            var targetVendorUserIds = new HashSet<int>();
            foreach (var user in vendorUsers)
            {
                if (targetVendorUserIds.Add(user.UserID))
                {
                    await _notificationRepository.AddAsync(new Notification
                    {
                        UserID = user.UserID,
                        Title = vendorTitle,
                        Message = vendorMsg,
                        NotificationType = "PurchaseOrderSent",
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
            ApproverRole = purchaseOrder.ApproverRole,
            Items = purchaseOrder.Items?.Select(item => new PurchaseOrderItemDto
            {
                POItemID = item.POItemID,
                ProductID = item.ProductID,
                Quantity = item.Quantity,
                UnitPrice = item.UnitPrice,
                TaxRate = item.TaxRate,
                Subtotal = item.Subtotal,
                TaxAmount = item.TaxAmount,
                TotalAmount = item.TotalAmount
            }).ToList() ?? new List<PurchaseOrderItemDto>()
        };
    }
}
