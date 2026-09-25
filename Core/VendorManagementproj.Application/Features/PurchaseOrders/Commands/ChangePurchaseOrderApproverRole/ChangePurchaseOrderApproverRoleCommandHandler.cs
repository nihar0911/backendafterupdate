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

namespace VendorManagementproj.Application.Features.PurchaseOrders.Commands.ChangePurchaseOrderApproverRole;

public class ChangePurchaseOrderApproverRoleCommandHandler : IRequestHandler<ChangePurchaseOrderApproverRoleCommand, ChangePurchaseOrderApproverRoleResponse>
{
    private readonly IPurchaseOrderRepository _purchaseOrderRepository;
    private readonly IPurchaseRequestRepository _purchaseRequestRepository;
    private readonly IVendorRepository _vendorRepository;
    private readonly INotificationRepository _notificationRepository;
    private readonly IUserRepository _userRepository;
    private readonly IOutletRepository _outletRepository;
    private readonly ICurrentUserService _currentUserService;

    public ChangePurchaseOrderApproverRoleCommandHandler(
        IPurchaseOrderRepository purchaseOrderRepository,
        IPurchaseRequestRepository purchaseRequestRepository,
        IVendorRepository vendorRepository,
        INotificationRepository notificationRepository,
        IUserRepository userRepository,
        IOutletRepository outletRepository,
        ICurrentUserService currentUserService)
    {
        _purchaseOrderRepository = purchaseOrderRepository;
        _purchaseRequestRepository = purchaseRequestRepository;
        _vendorRepository = vendorRepository;
        _notificationRepository = notificationRepository;
        _userRepository = userRepository;
        _outletRepository = outletRepository;
        _currentUserService = currentUserService;
    }

    public async Task<ChangePurchaseOrderApproverRoleResponse> Handle(
        ChangePurchaseOrderApproverRoleCommand request,
        CancellationToken cancellationToken)
    {
        if (request.PurchaseOrderID <= 0)
        {
            throw new InvalidOperationException("A valid purchase order ID is required.");
        }

        // Validate approver role
        if (string.IsNullOrWhiteSpace(request.ApproverRole))
        {
            throw new InvalidOperationException("Approver role is required.");
        }

        var trimmedRole = request.ApproverRole.Trim();
        if (!string.Equals(trimmedRole, PurchaseOrderApprover.OrganizationManager, StringComparison.OrdinalIgnoreCase) &&
            !string.Equals(trimmedRole, PurchaseOrderApprover.OutletManager, StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException($"Invalid approver role '{request.ApproverRole}'. Allowed roles are '{PurchaseOrderApprover.OrganizationManager}' or '{PurchaseOrderApprover.OutletManager}'.");
        }

        var normalizedRole = PurchaseOrderApprover.Normalize(trimmedRole);

        // Load Purchase Order
        var purchaseOrder = await _purchaseOrderRepository.GetByIdAsync(request.PurchaseOrderID);
        if (purchaseOrder == null)
        {
            throw new InvalidOperationException("Purchase order does not exist.");
        }

        // Status must be "Awaiting Approval"
        if (!string.Equals(purchaseOrder.Status, "Awaiting Approval", StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException($"Approver role can only be changed when the purchase order is awaiting approval. Current status is '{purchaseOrder.Status}'.");
        }

        // Authorization: Admin allowed for any outlet; Purchase Manager only for assigned OutletID
        if (!_currentUserService.IsAdmin && !_currentUserService.IsPurchaseManager)
        {
            throw new UnauthorizedAccessException("Only Purchase Managers or Admins can change the purchase order approver authority.");
        }

        if (_currentUserService.IsPurchaseManager)
        {
            if (!_currentUserService.OutletID.HasValue)
            {
                throw new UnauthorizedAccessException("You are not assigned to an outlet.");
            }

            if (purchaseOrder.OutletID != _currentUserService.OutletID.Value)
            {
                throw new UnauthorizedAccessException("You are not authorized to modify purchase orders belonging to another outlet.");
            }
        }

        // If the selected role is already the current role, return without saving or altering notifications
        var currentNormalizedRole = PurchaseOrderApprover.Normalize(purchaseOrder.ApproverRole);
        if (string.Equals(currentNormalizedRole, normalizedRole, StringComparison.OrdinalIgnoreCase))
        {
            return new ChangePurchaseOrderApproverRoleResponse
            {
                PurchaseOrder = MapToDto(purchaseOrder)
            };
        }

        // Update approver role and persist
        purchaseOrder.ApproverRole = normalizedRole;
        var updatedPurchaseOrder = await _purchaseOrderRepository.UpdateAsync(purchaseOrder);

        if (updatedPurchaseOrder == null)
        {
            throw new InvalidOperationException("Unable to update purchase order.");
        }

        // Notification Synchronization: Invalidate old unread notifications and notify newly selected approvers
        try
        {
            // Remove unread approval notifications for this specific PO
            await _notificationRepository.RemoveUnreadByRelatedRequestIdAsync(
                "PurchaseOrderAwaitingApproval",
                purchaseOrder.PurchaseOrderID);

            // Fetch outlet to get OrganizationID for Organization Manager lookup
            var outlet = await _outletRepository.GetByIdAsync(purchaseOrder.OutletID);
            var allUsers = await _userRepository.GetAllAsync();

            var approvers = allUsers.Where(u =>
            {
                var roleName = u.Role?.RoleName ?? string.Empty;
                if (normalizedRole == PurchaseOrderApprover.OutletManager)
                {
                    return u.OutletID == purchaseOrder.OutletID &&
                           string.Equals(roleName, PurchaseOrderApprover.OutletManager, StringComparison.OrdinalIgnoreCase);
                }

                return outlet != null &&
                       u.OrganizationID == outlet.OrganizationID &&
                       string.Equals(roleName, PurchaseOrderApprover.OrganizationManager, StringComparison.OrdinalIgnoreCase);
            }).ToList();

            var purchaseRequest = await _purchaseRequestRepository.GetByIdAsync(purchaseOrder.RequestID);
            var vendor = await _vendorRepository.GetByIdAsync(purchaseOrder.VendorID);
            string vendorName = vendor?.VendorName ?? "Vendor";
            var poProducts = purchaseOrder.Items?.Select(poi =>
            {
                var prItem = purchaseRequest?.Items?.FirstOrDefault(pi => pi.ProductID == poi.ProductID);
                var name = poi.Product?.ProductName ?? prItem?.Product?.ProductName ?? $"Product #{poi.ProductID}";
                var unit = poi.Product?.Unit ?? prItem?.Unit ?? prItem?.Product?.Unit;
                return ((string?)name, poi.Quantity, (string?)unit);
            });
            var (titleProd, msgProd) = NotificationProductFormatter.FormatProductSummaries(poProducts);

            string title = string.IsNullOrEmpty(titleProd)
                ? $"Purchase Order Awaiting Approval: PO-{purchaseOrder.PurchaseOrderID}"
                : $"Purchase Order Awaiting Approval: {titleProd}";

            string message = string.IsNullOrEmpty(msgProd)
                ? $"Purchase Order PO-{purchaseOrder.PurchaseOrderID} with {vendorName} is awaiting your approval before it is placed with the vendor."
                : $"Purchase Order PO-{purchaseOrder.PurchaseOrderID} for {msgProd} with {vendorName} is awaiting your approval before it is placed with the vendor.";

            var targetUserIds = new HashSet<int>();
            foreach (var user in approvers)
            {
                if (targetUserIds.Add(user.UserID))
                {
                    await _notificationRepository.AddAsync(new Notification
                    {
                        UserID = user.UserID,
                        Title = title,
                        Message = message,
                        NotificationType = "PurchaseOrderAwaitingApproval",
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
            Console.WriteLine($"[ChangePOApproverRole Notification Error]: {ex.Message}");
        }

        return new ChangePurchaseOrderApproverRoleResponse
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
