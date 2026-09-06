using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using VendorManagementprojApplication.Contracts.Persistence;
using VendorManagementprojApplication.Contracts.Services;
using VendorManagementprojDomain.Entities;

namespace VendorManagementprojApplication.Features.PurchaseRequests.Commands.DispatchPurchaseRequest;

public class DispatchPurchaseRequestCommandHandler
    : IRequestHandler<DispatchPurchaseRequestCommand, DispatchPurchaseRequestResponse>
{
    private readonly IPurchaseRequestRepository _purchaseRequestRepository;
    private readonly IVendorRepository _vendorRepository;
    private readonly IVendorProductRepository _vendorProductRepository;
    private readonly IProductRepository _productRepository;
    private readonly IOutletRepository _outletRepository;
    private readonly IUserRepository _userRepository;
    private readonly IVendorOpportunityResponseRepository _opportunityResponseRepository;
    private readonly INotificationRepository _notificationRepository;
    private readonly ICurrentUserService _currentUserService;

    public DispatchPurchaseRequestCommandHandler(
        IPurchaseRequestRepository purchaseRequestRepository,
        IVendorRepository vendorRepository,
        IVendorProductRepository vendorProductRepository,
        IProductRepository productRepository,
        IOutletRepository outletRepository,
        IUserRepository userRepository,
        IVendorOpportunityResponseRepository opportunityResponseRepository,
        INotificationRepository notificationRepository,
        ICurrentUserService currentUserService)
    {
        _purchaseRequestRepository = purchaseRequestRepository;
        _vendorRepository = vendorRepository;
        _vendorProductRepository = vendorProductRepository;
        _productRepository = productRepository;
        _outletRepository = outletRepository;
        _userRepository = userRepository;
        _opportunityResponseRepository = opportunityResponseRepository;
        _notificationRepository = notificationRepository;
        _currentUserService = currentUserService;
    }

    public async Task<DispatchPurchaseRequestResponse> Handle(
        DispatchPurchaseRequestCommand request,
        CancellationToken cancellationToken)
    {
        if (request.RequestID <= 0)
            throw new InvalidOperationException("A valid Purchase Request ID is required.");

        var purchaseRequest = await _purchaseRequestRepository.GetByIdAsync(request.RequestID);
        if (purchaseRequest == null)
            throw new KeyNotFoundException($"Purchase Request #{request.RequestID} does not exist.");

        if (string.Equals(purchaseRequest.Status, "Cancelled", StringComparison.OrdinalIgnoreCase) ||
            string.Equals(purchaseRequest.Status, "Completed", StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException($"Cannot dispatch a Purchase Request in '{purchaseRequest.Status}' status.");
        }

        if ((_currentUserService.IsOutletManager || _currentUserService.IsPurchaseManager) && _currentUserService.OutletID.HasValue)
        {
            if (purchaseRequest.OutletID != _currentUserService.OutletID.Value)
            {
                throw new UnauthorizedAccessException("You are not authorized to dispatch a purchase request belonging to another outlet.");
            }
        }
        else if (_currentUserService.IsPurchaseManager && !_currentUserService.OutletID.HasValue)
        {
            throw new UnauthorizedAccessException("You are not assigned to an outlet.");
        }
        else if (_currentUserService.IsOrganizationManager && _currentUserService.OrganizationID.HasValue)
        {
            var orgOutlets = await _outletRepository.GetByOrganizationIdAsync(_currentUserService.OrganizationID.Value);
            var orgOutletIds = orgOutlets.Select(o => o.OutletID).ToHashSet();
            if (!orgOutletIds.Contains(purchaseRequest.OutletID))
            {
                throw new UnauthorizedAccessException("You are not authorized to dispatch a purchase request outside your organization.");
            }
        }

        var selectedVendorIds = request.SelectedVendorIDs
            .Where(v => v > 0)
            .Distinct()
            .ToList();

        if (selectedVendorIds.Count == 0)
            throw new InvalidOperationException("At least one selected vendor is required.");

        var targetOutlet = await _outletRepository.GetByIdAsync(purchaseRequest.OutletID);
        string outletName = !string.IsNullOrWhiteSpace(targetOutlet?.OutletName) ? targetOutlet.OutletName : $"Outlet #{purchaseRequest.OutletID}";

        var allUsers = await _userRepository.GetAllAsync();
        int oppsCreated = 0;
        int notifsSent = 0;

        foreach (var vendorId in selectedVendorIds)
        {
            var vendor = await _vendorRepository.GetByIdAsync(vendorId);
            if (vendor == null)
                throw new InvalidOperationException($"Vendor ID #{vendorId} does not exist.");

            if (!string.Equals(vendor.Status, "Active", StringComparison.OrdinalIgnoreCase))
                throw new InvalidOperationException($"Vendor '{vendor.VendorName}' (ID #{vendorId}) is not active.");

            foreach (var item in purchaseRequest.Items)
            {
                var vendorProduct = await _vendorProductRepository.GetByVendorAndProductAsync(vendorId, item.ProductID);
                if (vendorProduct == null || !string.Equals(vendorProduct.Status, "Active", StringComparison.OrdinalIgnoreCase))
                {
                    var p = await _productRepository.GetByIdAsync(item.ProductID);
                    throw new InvalidOperationException($"Vendor '{vendor.VendorName}' does not supply product '{p?.ProductName ?? $"#{item.ProductID}"}'.");
                }

                var existingResp = await _opportunityResponseRepository.GetByRequestAndVendorAsync(
                    purchaseRequest.RequestID, vendorId, item.ProductID);

                if (existingResp == null)
                {
                    var opp = new VendorOpportunityResponse
                    {
                        RequestID = purchaseRequest.RequestID,
                        VendorID = vendorId,
                        ProductID = item.ProductID,
                        Status = "Pending",
                        CreatedDate = DateTime.UtcNow
                    };
                    await _opportunityResponseRepository.AddAsync(opp);
                    oppsCreated++;
                }

                // Send Notification ONLY to the Vendor Manager(s) assigned to THIS selected vendor
                var vendorManagers = allUsers.Where(u =>
                    u.VendorID == vendorId &&
                    (string.Equals(u.Role?.RoleName, "Vendor Manager", StringComparison.OrdinalIgnoreCase) || u.RoleID == 4))
                    .ToList();

                var product = await _productRepository.GetByIdAsync(item.ProductID);
                string prodName = product?.ProductName ?? $"Product #{item.ProductID}";

                foreach (var vm in vendorManagers)
                {
                    var notif = new Notification
                    {
                        UserID = vm.UserID,
                        Title = $"New Procurement Opportunity #PR-{purchaseRequest.RequestID}",
                        Message = $"You have received a new procurement opportunity for {item.Quantity:N2} {item.Unit} of {prodName} from {outletName}.",
                        NotificationType = "ProcurementOpportunity",
                        RelatedRequestID = purchaseRequest.RequestID, RelatedVendorID = vendorId,
                        IsRead = false,
                        CreatedDate = DateTime.UtcNow
                    };
                    await _notificationRepository.AddAsync(notif);
                    notifsSent++;
                }
            }
        }

        return new DispatchPurchaseRequestResponse
        {
            Success = true,
            Message = $"Purchase Request #{purchaseRequest.RequestID} dispatched to {selectedVendorIds.Count} selected vendor(s).",
            OpportunitiesCreated = oppsCreated,
            NotificationsSent = notifsSent
        };
    }
}
