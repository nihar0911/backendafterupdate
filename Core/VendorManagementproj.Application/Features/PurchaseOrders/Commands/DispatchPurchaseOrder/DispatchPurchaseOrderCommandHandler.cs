using VendorManagementproj.Application.Common;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using VendorManagementproj.Application.Contracts.Persistence;
using VendorManagementproj.Application.DTOs;
using VendorManagementproj.Domain.Entities;

namespace VendorManagementproj.Application.Features.PurchaseOrders.Commands.DispatchPurchaseOrder;

public class DispatchPurchaseOrderCommandHandler : IRequestHandler<DispatchPurchaseOrderCommand, DispatchPurchaseOrderResponse>
{
    private readonly IPurchaseOrderRepository _purchaseOrderRepository;
    private readonly IPurchaseRequestRepository _purchaseRequestRepository;
    private readonly INotificationRepository _notificationRepository;
    private readonly IUserRepository _userRepository;
    private readonly IOutletRepository _outletRepository;
    private readonly IVendorRepository _vendorRepository;

    public DispatchPurchaseOrderCommandHandler(
        IPurchaseOrderRepository purchaseOrderRepository,
        IPurchaseRequestRepository purchaseRequestRepository,
        INotificationRepository notificationRepository,
        IUserRepository userRepository,
        IOutletRepository outletRepository,
        IVendorRepository vendorRepository)
    {
        _purchaseOrderRepository = purchaseOrderRepository;
        _purchaseRequestRepository = purchaseRequestRepository;
        _notificationRepository = notificationRepository;
        _userRepository = userRepository;
        _outletRepository = outletRepository;
        _vendorRepository = vendorRepository;
    }

    public async Task<DispatchPurchaseOrderResponse> Handle(
        DispatchPurchaseOrderCommand request,
        CancellationToken cancellationToken)
    {
        var purchaseOrder = await _purchaseOrderRepository.GetByIdAsync(request.PurchaseOrderID);

        if (purchaseOrder == null)
            throw new InvalidOperationException("Purchase order does not exist.");

        if (purchaseOrder.VendorID != request.VendorID)
            throw new UnauthorizedAccessException("This purchase order does not belong to this vendor.");

        if (!string.Equals(purchaseOrder.Status, "Accepted", StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException("Only an accepted purchase order can be dispatched.");

        purchaseOrder.Status = "Dispatched";
        purchaseOrder.DispatchDateTime = DateTime.Now;

        var updatedPurchaseOrder = await _purchaseOrderRepository.UpdateAsync(purchaseOrder);

        if (updatedPurchaseOrder == null)
            throw new InvalidOperationException("Unable to update purchase order.");

        // Resolve vendor name dynamically
        var vendor = await _vendorRepository.GetByIdAsync(purchaseOrder.VendorID);
        string vendorName = vendor?.VendorName ?? "Vendor";

        // Notify Stakeholders (Outlet Manager AND Organization Manager)
        try
        {
            var allUsers = await _userRepository.GetAllAsync();
            var targetOutletUserIds = new HashSet<int>();
            var targetOrgUserIds = new HashSet<int>();

            // 1. Notify Outlet Manager(s) and Purchase Manager(s) for this Outlet
            var outletUsers = allUsers.Where(u => u.OutletID == purchaseOrder.OutletID && (string.Equals(u.Role?.RoleName, "Outlet Manager", StringComparison.OrdinalIgnoreCase) || string.Equals(u.Role?.RoleName, "Purchase Manager", StringComparison.OrdinalIgnoreCase) || u.RoleID == 3 || u.RoleID == 7));
            foreach (var user in outletUsers)
            {
                targetOutletUserIds.Add(user.UserID);
            }

            // 2. Notify Organization Manager(s)
            var outlet = await _outletRepository.GetByIdAsync(purchaseOrder.OutletID);
            if (outlet != null)
            {
                var orgUsers = allUsers.Where(u => u.OrganizationID == outlet.OrganizationID && (string.Equals(u.Role?.RoleName, "Organization Manager", StringComparison.OrdinalIgnoreCase) || u.RoleID == 2));
                foreach (var user in orgUsers)
                {
                    if (!targetOutletUserIds.Contains(user.UserID))
                    {
                        targetOrgUserIds.Add(user.UserID);
                    }
                }
            }

            var purchaseRequest = await _purchaseRequestRepository.GetByIdAsync(purchaseOrder.RequestID);
            var poProducts = purchaseOrder.Items?.Select(poi =>
            {
                var prItem = purchaseRequest?.Items?.FirstOrDefault(pi => pi.ProductID == poi.ProductID);
                var name = poi.Product?.ProductName ?? prItem?.Product?.ProductName ?? $"Product #{poi.ProductID}";
                var unit = poi.Product?.Unit ?? prItem?.Unit ?? prItem?.Product?.Unit;
                return ((string?)name, poi.Quantity, (string?)unit);
            });
            var (prodTitle, prodMsg) = NotificationProductFormatter.FormatProductSummaries(poProducts);

            string notifTitle = string.IsNullOrWhiteSpace(prodTitle) ? "Purchase Order Dispatched" : $"Purchase Order Dispatched: {prodTitle}";
            string outletMsg = string.IsNullOrWhiteSpace(prodMsg)
                ? $"Purchase Order PO-{purchaseOrder.PurchaseOrderID} has been dispatched and is on the way to your outlet."
                : $"Purchase Order PO-{purchaseOrder.PurchaseOrderID} for {prodMsg} has been dispatched and is on the way to your outlet.";
            string orgMsg = string.IsNullOrWhiteSpace(prodMsg)
                ? $"Purchase Order PO-{purchaseOrder.PurchaseOrderID} has been dispatched by {vendorName}."
                : $"Purchase Order PO-{purchaseOrder.PurchaseOrderID} for {prodMsg} has been dispatched by {vendorName}.";

            foreach (var userId in targetOutletUserIds)
            {
                await _notificationRepository.AddAsync(new Notification
                {
                    UserID = userId,
                    Title = notifTitle,
                    Message = outletMsg,
                    NotificationType = "PurchaseOrderDispatched",
                    RelatedRequestID = purchaseOrder.PurchaseOrderID,
                    RelatedVendorID = purchaseOrder.VendorID,
                    IsRead = false,
                    CreatedDate = DateTime.Now
                });
            }

            foreach (var userId in targetOrgUserIds)
            {
                await _notificationRepository.AddAsync(new Notification
                {
                    UserID = userId,
                    Title = notifTitle,
                    Message = orgMsg,
                    NotificationType = "PurchaseOrderDispatched",
                    RelatedRequestID = purchaseOrder.PurchaseOrderID,
                    RelatedVendorID = purchaseOrder.VendorID,
                    IsRead = false,
                    CreatedDate = DateTime.Now
                });
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[DispatchPO Notification Error]: {ex.Message}");
        }

        return new DispatchPurchaseOrderResponse
        {
            PurchaseOrder = MapToDto(updatedPurchaseOrder)
        };
    }

    private static PurchaseOrderDto MapToDto(
        VendorManagementproj.Domain.Entities.PurchaseOrder purchaseOrder)
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
            }).ToList() ?? new System.Collections.Generic.List<PurchaseOrderItemDto>()
        };
    }
}
