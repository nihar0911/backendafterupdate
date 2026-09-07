using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using VendorManagementprojApplication.Contracts.Persistence;
using VendorManagementprojApplication.DTOs;
using VendorManagementprojDomain.Entities;

namespace VendorManagementprojApplication.Features.PurchaseOrders.Commands.DispatchPurchaseOrder;

public class DispatchPurchaseOrderCommandHandler : IRequestHandler<DispatchPurchaseOrderCommand, DispatchPurchaseOrderResponse>
{
    private readonly IPurchaseOrderRepository _purchaseOrderRepository;
    private readonly INotificationRepository _notificationRepository;
    private readonly IUserRepository _userRepository;
    private readonly IOutletRepository _outletRepository;
    private readonly IVendorRepository _vendorRepository;

    public DispatchPurchaseOrderCommandHandler(
        IPurchaseOrderRepository purchaseOrderRepository,
        INotificationRepository notificationRepository,
        IUserRepository userRepository,
        IOutletRepository outletRepository,
        IVendorRepository vendorRepository)
    {
        _purchaseOrderRepository = purchaseOrderRepository;
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

            // 1. Notify Outlet Manager(s) for this Outlet
            var outletUsers = allUsers.Where(u => u.OutletID == purchaseOrder.OutletID).ToList();
            foreach (var user in outletUsers)
            {
                await _notificationRepository.AddAsync(new Notification
                {
                    UserID = user.UserID,
                    Title = "Purchase Order Dispatched",
                    Message = $"Purchase Order PO-#{purchaseOrder.PurchaseOrderID} has been dispatched and is on the way to your outlet.",
                    NotificationType = "PurchaseOrderDispatched",
                    RelatedRequestID = purchaseOrder.PurchaseOrderID,
                    RelatedVendorID = purchaseOrder.VendorID,
                    IsRead = false,
                    CreatedDate = DateTime.UtcNow
                });
            }

            // 2. Notify Organization Manager(s)
            var outlet = await _outletRepository.GetByIdAsync(purchaseOrder.OutletID);
            if (outlet != null)
            {
                var orgUsers = allUsers.Where(u => u.OrganizationID == outlet.OrganizationID).ToList();
                foreach (var user in orgUsers)
                {
                    await _notificationRepository.AddAsync(new Notification
                    {
                        UserID = user.UserID,
                        Title = "Purchase Order Dispatched",
                        Message = $"Purchase Order PO-#{purchaseOrder.PurchaseOrderID} has been dispatched by {vendorName}.",
                        NotificationType = "PurchaseOrderDispatched",
                        RelatedRequestID = purchaseOrder.PurchaseOrderID,
                        RelatedVendorID = purchaseOrder.VendorID,
                        IsRead = false,
                        CreatedDate = DateTime.UtcNow
                    });
                }
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
        VendorManagementprojDomain.Entities.PurchaseOrder purchaseOrder)
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
