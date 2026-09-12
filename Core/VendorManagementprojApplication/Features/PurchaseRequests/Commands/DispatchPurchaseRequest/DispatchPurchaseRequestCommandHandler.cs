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
            throw new UnauthorizedAccessException("You are not authorized to dispatch this purchase request.");
        }

        if (purchaseRequest.Items == null || purchaseRequest.Items.Count == 0)
        {
            throw new InvalidOperationException("Purchase Request does not contain any items.");
        }

        var targetOutlet = await _outletRepository.GetByIdAsync(purchaseRequest.OutletID);
        string outletName = !string.IsNullOrWhiteSpace(targetOutlet?.OutletName) ? targetOutlet.OutletName : $"Outlet #{purchaseRequest.OutletID}";

        var allUsers = await _userRepository.GetAllAsync();
        int oppsCreated = 0;
        int notifsSent = 0;

        // Resolved assignment entries: (item, vendor)
        var resolvedPairs = new List<(PurchaseRequestItem Item, Vendor Vendor)>();
        var distinctKeys = new HashSet<string>();

        if (request.ItemVendorAssignments != null && request.ItemVendorAssignments.Count > 0)
        {
            // Explicit Item-to-Vendor Routing
            foreach (var assignment in request.ItemVendorAssignments)
            {
                if (assignment.VendorID <= 0)
                    throw new InvalidOperationException("A valid vendor ID is required for each item assignment.");

                PurchaseRequestItem? item = null;
                if (assignment.RequestItemID.HasValue && assignment.RequestItemID.Value > 0)
                {
                    item = purchaseRequest.Items.FirstOrDefault(i => i.RequestItemID == assignment.RequestItemID.Value);
                    if (item != null && assignment.ProductID > 0 && item.ProductID != assignment.ProductID)
                    {
                        throw new InvalidOperationException($"RequestItemID #{assignment.RequestItemID.Value} does not match ProductID #{assignment.ProductID}.");
                    }
                }
                else
                {
                    var matchingItems = purchaseRequest.Items.Where(i => i.ProductID == assignment.ProductID).ToList();
                    if (matchingItems.Count > 1)
                    {
                        throw new InvalidOperationException($"Purchase Request #{purchaseRequest.RequestID} contains multiple line items for Product ID #{assignment.ProductID}. A valid RequestItemID must be specified.");
                    }
                    item = matchingItems.FirstOrDefault();
                }

                if (item == null)
                    throw new InvalidOperationException($"Product ID #{assignment.ProductID} is not part of Purchase Request #{purchaseRequest.RequestID}.");

                var vendor = await _vendorRepository.GetByIdAsync(assignment.VendorID);
                if (vendor == null)
                    throw new InvalidOperationException($"Vendor ID #{assignment.VendorID} does not exist.");

                if (!string.Equals(vendor.Status, "Active", StringComparison.OrdinalIgnoreCase))
                    throw new InvalidOperationException($"Vendor '{vendor.VendorName}' (ID #{assignment.VendorID}) is not active.");

                var vendorProduct = await _vendorProductRepository.GetByVendorAndProductAsync(assignment.VendorID, item.ProductID);
                if (vendorProduct == null || !string.Equals(vendorProduct.Status, "Active", StringComparison.OrdinalIgnoreCase))
                {
                    var p = await _productRepository.GetByIdAsync(item.ProductID);
                    throw new InvalidOperationException($"Vendor '{vendor.VendorName}' does not supply product '{p?.ProductName ?? $"#{item.ProductID}"}'.");
                }

                string key = $"{item.RequestItemID}_{vendor.VendorID}";
                if (distinctKeys.Add(key))
                {
                    resolvedPairs.Add((item, vendor));
                }
            }
        }
        else
        {
            // Legacy SelectedVendorIDs Routing
            var selectedVendorIds = (request.SelectedVendorIDs ?? new List<int>())
                .Where(v => v > 0)
                .Distinct()
                .ToList();

            if (selectedVendorIds.Count == 0)
                throw new InvalidOperationException("At least one selected vendor or item-vendor assignment is required.");

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

                    if (purchaseRequest.Items.Count == 1)
                    {
                        if (vendorProduct == null || !string.Equals(vendorProduct.Status, "Active", StringComparison.OrdinalIgnoreCase))
                        {
                            var p = await _productRepository.GetByIdAsync(item.ProductID);
                            throw new InvalidOperationException($"Vendor '{vendor.VendorName}' does not supply product '{p?.ProductName ?? $"#{item.ProductID}"}'.");
                        }
                        string key = $"{item.RequestItemID}_{vendor.VendorID}";
                        if (distinctKeys.Add(key))
                        {
                            resolvedPairs.Add((item, vendor));
                        }
                    }
                    else
                    {
                        if (vendorProduct != null && string.Equals(vendorProduct.Status, "Active", StringComparison.OrdinalIgnoreCase))
                        {
                            string key = $"{item.RequestItemID}_{vendor.VendorID}";
                            if (distinctKeys.Add(key))
                            {
                                resolvedPairs.Add((item, vendor));
                            }
                        }
                    }
                }
            }

            if (resolvedPairs.Count == 0)
                throw new InvalidOperationException("None of the selected vendors supply the products in this purchase request.");
        }

        // Create opportunities and notifications only for resolved (item, vendor) pairs
        foreach (var (item, vendor) in resolvedPairs)
        {
            var existingResp = await _opportunityResponseRepository.GetByRequestItemAndVendorAsync(
                item.RequestItemID, vendor.VendorID);

            if (existingResp == null)
            {
                var opp = new VendorOpportunityResponse
                {
                    RequestID = purchaseRequest.RequestID,
                    RequestItemID = item.RequestItemID,
                    VendorID = vendor.VendorID,
                    ProductID = item.ProductID,
                    Status = "Pending",
                    CreatedDate = DateTime.UtcNow
                };
                await _opportunityResponseRepository.AddAsync(opp);
                oppsCreated++;
            }

            // Send Notification ONLY to the Vendor Manager(s) assigned to THIS specific vendor
            var vendorManagers = allUsers.Where(u =>
                u.VendorID == vendor.VendorID &&
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
                    RelatedRequestID = purchaseRequest.RequestID,
                    RelatedVendorID = vendor.VendorID,
                    IsRead = false,
                    CreatedDate = DateTime.UtcNow
                };
                await _notificationRepository.AddAsync(notif);
                notifsSent++;
            }
        }

        return new DispatchPurchaseRequestResponse
        {
            Success = true,
            Message = $"Purchase Request #{purchaseRequest.RequestID} dispatched successfully.",
            OpportunitiesCreated = oppsCreated,
            NotificationsSent = notifsSent
        };
    }
}
