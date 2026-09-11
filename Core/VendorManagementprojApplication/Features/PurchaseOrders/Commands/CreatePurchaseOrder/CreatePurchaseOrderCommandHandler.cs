using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using VendorManagementprojApplication.Common;
using VendorManagementprojApplication.Contracts.Persistence;
using VendorManagementprojApplication.Contracts.Services;
using VendorManagementprojApplication.DTOs;
using VendorManagementprojDomain.Entities;

namespace VendorManagementprojApplication.Features.PurchaseOrders.Commands.CreatePurchaseOrder;

public class CreatePurchaseOrderCommandHandler : IRequestHandler<CreatePurchaseOrderCommand, CreatePurchaseOrderResponse>
{
    private readonly IPurchaseOrderRepository _purchaseOrderRepository;
    private readonly IQuotationRepository _quotationRepository;
    private readonly IPurchaseRequestRepository _purchaseRequestRepository;
    private readonly IContractRepository _contractRepository;
    private readonly INotificationRepository _notificationRepository;
    private readonly IUserRepository _userRepository;
    private readonly IOutletRepository _outletRepository;
    private readonly ICurrentUserService _currentUserService;

    public CreatePurchaseOrderCommandHandler(
        IPurchaseOrderRepository purchaseOrderRepository,
        IQuotationRepository quotationRepository,
        IPurchaseRequestRepository purchaseRequestRepository,
        IContractRepository contractRepository,
        INotificationRepository notificationRepository,
        IUserRepository userRepository,
        IOutletRepository outletRepository,
        ICurrentUserService currentUserService)
       {
        _purchaseOrderRepository = purchaseOrderRepository;
        _quotationRepository = quotationRepository;
        _purchaseRequestRepository = purchaseRequestRepository;
        _contractRepository = contractRepository;
        _notificationRepository = notificationRepository;
        _userRepository = userRepository;
        _outletRepository = outletRepository;
        _currentUserService = currentUserService;
    }

    public async Task<CreatePurchaseOrderResponse> Handle(
        CreatePurchaseOrderCommand request,
        CancellationToken cancellationToken)
    {
        var quotation = await _quotationRepository.GetByIdAsync(request.QuotationID);

        if (quotation == null)
            throw new InvalidOperationException("Quotation does not exist.");

        if (!string.Equals(quotation.Status, "Accepted", StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException("Purchase order can only be created for an accepted quotation.");

        if (quotation.ValidUntil < DateTime.Now)
            throw new InvalidOperationException("The quotation has expired.");

        var existingPurchaseOrder = await _purchaseOrderRepository.GetByQuotationIdAsync(request.QuotationID);
        if (existingPurchaseOrder != null)
            throw new InvalidOperationException("A purchase order already exists for this quotation.");

        var purchaseRequest = await _purchaseRequestRepository.GetByIdAsync(quotation.RequestID);
        if (purchaseRequest == null)
            throw new InvalidOperationException("Purchase request does not exist.");

        // Security / Role & Outlet Authorization Check
        bool isAuthorized = false;
        if (_currentUserService.IsAdmin)
        {
            isAuthorized = true;
        }
        else if (_currentUserService.IsPurchaseManager && _currentUserService.OutletID.HasValue)
        {
            if (purchaseRequest.OutletID == _currentUserService.OutletID.Value)
            {
                isAuthorized = true;
            }
        }

        if (!isAuthorized)
        {
            throw new UnauthorizedAccessException("You are not authorized to create purchase orders for this outlet.");
        }

        if (quotation.QuotationItems == null || quotation.QuotationItems.Count == 0)
            throw new InvalidOperationException("Quotation must contain at least one item.");

        var outlet = await _outletRepository.GetByIdAsync(purchaseRequest.OutletID);
        if (outlet == null)
            throw new InvalidOperationException("Outlet does not exist.");

        var approverRole = PurchaseOrderApprover.Normalize(outlet.PurchaseOrderApproverRole);

        var purchaseOrder = new PurchaseOrder
        {
            RequestID = quotation.RequestID,
            VendorID = quotation.VendorID,
            QuotationID = quotation.QuotationID,
            OutletID = purchaseRequest.OutletID,
            OrderDate = DateTime.Now,
            ExpectedDeliveryDate = request.ExpectedDeliveryDate,
            ActualDeliveryDate = null,
            DeliveryStatus = null,
            Status = "Awaiting Approval",
            ApproverRole = approverRole,
            Items = new List<PurchaseOrderItem>()
        };

        foreach (var quotationItem in quotation.QuotationItems)
        {
            if (quotationItem.Quantity <= 0)
                throw new InvalidOperationException("Quotation item quantity must be greater than zero.");

            if (quotationItem.UnitPrice < 0)
                throw new InvalidOperationException("Quotation item unit price cannot be negative.");

            var subtotal = quotationItem.UnitPrice * quotationItem.Quantity;

            purchaseOrder.Items.Add(
                new PurchaseOrderItem
                {
                    ProductID = quotationItem.ProductID,
                    Quantity = quotationItem.Quantity,
                    UnitPrice = quotationItem.UnitPrice,
                    TaxRate = quotationItem.TaxRate,
                    Subtotal = subtotal,
                    TaxAmount = quotationItem.TaxAmount,
                    TotalAmount = quotationItem.TotalAmount
                });
        }

        var createdPurchaseOrder = await _purchaseOrderRepository.AddAsync(purchaseOrder);

        try
        {
            var allUsers = await _userRepository.GetAllAsync();
            var approvers = allUsers.Where(u =>
            {
                var roleName = u.Role?.RoleName ?? string.Empty;
                if (approverRole == PurchaseOrderApprover.OutletManager)
                {
                    return u.OutletID == purchaseRequest.OutletID &&
                           string.Equals(roleName, PurchaseOrderApprover.OutletManager, StringComparison.OrdinalIgnoreCase);
                }

                return u.OrganizationID == outlet.OrganizationID &&
                       string.Equals(roleName, PurchaseOrderApprover.OrganizationManager, StringComparison.OrdinalIgnoreCase);
            }).ToList();

            foreach (var user in approvers)
            {
                await _notificationRepository.AddAsync(new Notification
                {
                    UserID = user.UserID,
                    Title = "Purchase Order Awaiting Approval",
                    Message = $"Purchase Order PO-#{createdPurchaseOrder.PurchaseOrderID} is awaiting your approval before it is placed with the vendor.",
                    NotificationType = "PurchaseOrderAwaitingApproval",
                    RelatedRequestID = createdPurchaseOrder.PurchaseOrderID,
                    RelatedVendorID = quotation.VendorID,
                    IsRead = false,
                    CreatedDate = DateTime.UtcNow
                });
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[CreatePO Notification Error]: {ex.Message}");
        }

        return new CreatePurchaseOrderResponse
        {
            PurchaseOrder = MapToDto(createdPurchaseOrder)
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
            Items = purchaseOrder.Items.Select(item => new PurchaseOrderItemDto
            {
                POItemID = item.POItemID,
                ProductID = item.ProductID,
                Quantity = item.Quantity,
                UnitPrice = item.UnitPrice,
                TaxRate = item.TaxRate,
                Subtotal = item.Subtotal,
                TaxAmount = item.TaxAmount,
                TotalAmount = item.TotalAmount
            }).ToList()
        };
    }
}
