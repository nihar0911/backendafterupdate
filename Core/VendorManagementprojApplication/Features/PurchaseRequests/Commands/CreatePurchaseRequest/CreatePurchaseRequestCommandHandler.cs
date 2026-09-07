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

namespace VendorManagementprojApplication.Features.PurchaseRequests.Commands.CreatePurchaseRequest;

public class CreatePurchaseRequestCommandHandler
    : IRequestHandler<CreatePurchaseRequestCommand, CreatePurchaseRequestResponse>
{
    private readonly IPurchaseRequestRepository _purchaseRequestRepository;
    private readonly IUserRepository _userRepository;
    private readonly IProductRepository _productRepository;
    private readonly IOutletRepository _outletRepository;
    private readonly IVendorRepository _vendorRepository;
    private readonly IVendorProductRepository _vendorProductRepository;
    private readonly IVendorOpportunityResponseRepository _opportunityResponseRepository;
    private readonly INotificationRepository _notificationRepository;
    private readonly ICurrentUserService _currentUserService;

    public CreatePurchaseRequestCommandHandler(
        IPurchaseRequestRepository purchaseRequestRepository,
        IUserRepository userRepository,
        IProductRepository productRepository,
        IOutletRepository outletRepository,
        IVendorRepository vendorRepository,
        IVendorProductRepository vendorProductRepository,
        IVendorOpportunityResponseRepository opportunityResponseRepository,
        INotificationRepository notificationRepository,
        ICurrentUserService currentUserService)
    {
        _purchaseRequestRepository = purchaseRequestRepository;
        _userRepository = userRepository;
        _productRepository = productRepository;
        _outletRepository = outletRepository;
        _vendorRepository = vendorRepository;
        _vendorProductRepository = vendorProductRepository;
        _opportunityResponseRepository = opportunityResponseRepository;
        _notificationRepository = notificationRepository;
        _currentUserService = currentUserService;
    }

    public async Task<CreatePurchaseRequestResponse> Handle(
        CreatePurchaseRequestCommand request,
        CancellationToken cancellationToken)
    {
        if (request.OutletID <= 0)
            throw new InvalidOperationException(
                "A valid outlet is required.");

        int createdByUserId = _currentUserService.UserID ?? request.CreatedByUserID;

        if (createdByUserId <= 0)
            throw new InvalidOperationException(
                "A valid user is required.");

        if (request.Items == null || request.Items.Count == 0)
            throw new InvalidOperationException(
                "Purchase request must contain at least one item.");

        var user =
            await _userRepository.GetByIdAsync(
                createdByUserId);

        if (user == null)
            throw new InvalidOperationException(
                "User does not exist.");

        var targetOutlet = await _outletRepository.GetByIdAsync(request.OutletID);
        if (targetOutlet == null)
            throw new InvalidOperationException(
                "Target outlet does not exist.");

        bool isAuthorized = false;

        if (_currentUserService.IsAdmin || string.Equals(user.Role?.RoleName, "Admin", StringComparison.OrdinalIgnoreCase))
        {
            isAuthorized = true;
        }
        else if (_currentUserService.IsPurchaseManager || string.Equals(user.Role?.RoleName, "Purchase Manager", StringComparison.OrdinalIgnoreCase))
        {
            int? userOutletId = _currentUserService.OutletID ?? user.OutletID;
            if (userOutletId.HasValue && request.OutletID == userOutletId.Value)
            {
                isAuthorized = true;
            }
        }

        if (!isAuthorized)
            throw new UnauthorizedAccessException(
                "User is not authorized to create a purchase request for this outlet.");

        var purchaseRequest = new PurchaseRequest
        {
            OutletID = request.OutletID,
            CreatedByUserID = createdByUserId,
            RequestDate = DateTime.Now,
            Status = "Pending",
            Items = new List<PurchaseRequestItem>()
        };

        var loadedProducts = new Dictionary<int, Product>();

        foreach (var itemDto in request.Items)
        {
            if (itemDto.ProductID <= 0)
                throw new InvalidOperationException(
                    "A valid product is required.");

            if (itemDto.Quantity <= 0)
                throw new InvalidOperationException(
                    "Quantity must be greater than zero.");

            if (string.IsNullOrWhiteSpace(itemDto.Unit))
                throw new InvalidOperationException(
                    "Unit is required.");

            var product =
                await _productRepository.GetByIdAsync(
                    itemDto.ProductID);

            if (product == null)
                throw new InvalidOperationException(
                    $"ProductID {itemDto.ProductID} does not exist.");

            if (!string.Equals(product.Status, "Active", StringComparison.OrdinalIgnoreCase))
                throw new InvalidOperationException(
                    $"ProductID {itemDto.ProductID} is not active.");

            if (itemDto.VendorID.HasValue && itemDto.VendorID.Value > 0)
            {
                var vendor = await _vendorRepository.GetByIdAsync(itemDto.VendorID.Value);
                if (vendor == null)
                    throw new InvalidOperationException(
                        $"Vendor ID #{itemDto.VendorID.Value} does not exist.");

                if (!string.Equals(vendor.Status, "Active", StringComparison.OrdinalIgnoreCase))
                    throw new InvalidOperationException(
                        $"Vendor '{vendor.VendorName}' (ID #{itemDto.VendorID.Value}) is not active.");

                var vendorProduct = await _vendorProductRepository.GetByVendorAndProductAsync(
                    itemDto.VendorID.Value, itemDto.ProductID);

                if (vendorProduct == null || !string.Equals(vendorProduct.Status, "Active", StringComparison.OrdinalIgnoreCase))
                {
                    throw new InvalidOperationException(
                        $"Vendor '{vendor.VendorName}' does not supply product '{product.ProductName}'.");
                }
            }

            loadedProducts[itemDto.ProductID] = product;

            purchaseRequest.Items.Add(
                new PurchaseRequestItem
                {
                    ProductID = itemDto.ProductID,
                    Quantity = itemDto.Quantity,
                    Unit = itemDto.Unit
                });
        }

        var createdRequest =
            await _purchaseRequestRepository
                .AddAsync(purchaseRequest);

        var allUsers = await _userRepository.GetAllAsync();
        string outletName = !string.IsNullOrWhiteSpace(targetOutlet.OutletName) ? targetOutlet.OutletName : $"Outlet #{targetOutlet.OutletID}";

        var itemVendorAssignments = request.Items
            .Where(i => i.VendorID.HasValue && i.VendorID.Value > 0)
            .ToList();

        if (itemVendorAssignments.Count > 0)
        {
            // Explicit Item-Level Vendor Assignment Routing
            var processedPairs = new HashSet<string>();

            foreach (var itemDto in itemVendorAssignments)
            {
                int vendorId = itemDto.VendorID!.Value;
                int productId = itemDto.ProductID;
                string pairKey = $"{vendorId}_{productId}";

                if (!processedPairs.Add(pairKey))
                    continue;

                var existingResp = await _opportunityResponseRepository.GetByRequestAndVendorAsync(
                    createdRequest.RequestID, vendorId, productId);

                if (existingResp == null)
                {
                    var opp = new VendorOpportunityResponse
                    {
                        RequestID = createdRequest.RequestID,
                        VendorID = vendorId,
                        ProductID = productId,
                        Status = "Pending",
                        CreatedDate = DateTime.UtcNow
                    };
                    await _opportunityResponseRepository.AddAsync(opp);
                }

                var vendorManagers = allUsers.Where(u =>
                    u.VendorID == vendorId &&
                    (string.Equals(u.Role?.RoleName, "Vendor Manager", StringComparison.OrdinalIgnoreCase) || u.RoleID == 4))
                    .ToList();

                loadedProducts.TryGetValue(productId, out var prod);
                string prodName = prod?.ProductName ?? $"Product #{productId}";
                var matchingItem = createdRequest.Items.FirstOrDefault(i => i.ProductID == productId);
                decimal qty = matchingItem?.Quantity ?? itemDto.Quantity;
                string unit = matchingItem?.Unit ?? itemDto.Unit;

                foreach (var vm in vendorManagers)
                {
                    var notif = new Notification
                    {
                        UserID = vm.UserID,
                        Title = $"New Procurement Opportunity #PR-{createdRequest.RequestID}",
                        Message = $"You have received a new procurement opportunity for {qty:N2} {unit} of {prodName} from {outletName}.",
                        NotificationType = "ProcurementOpportunity",
                        RelatedRequestID = createdRequest.RequestID,
                        RelatedVendorID = vendorId,
                        IsRead = false,
                        CreatedDate = DateTime.UtcNow
                    };
                    await _notificationRepository.AddAsync(notif);
                }
            }
        }
        else
        {
            // Legacy Fallback: Resolve selected vendors (distinct positive vendor IDs)
            var selectedVendorIds = new HashSet<int>();
            if (request.SelectedVendorIDs != null && request.SelectedVendorIDs.Count > 0)
            {
                foreach (var vid in request.SelectedVendorIDs.Where(v => v > 0))
                {
                    selectedVendorIds.Add(vid);
                }
            }
            if (request.SelectedVendorID.HasValue && request.SelectedVendorID.Value > 0)
            {
                selectedVendorIds.Add(request.SelectedVendorID.Value);
            }

            if (selectedVendorIds.Count > 0)
            {
                foreach (var vendorId in selectedVendorIds)
                {
                    var vendor = await _vendorRepository.GetByIdAsync(vendorId);
                    if (vendor == null || !string.Equals(vendor.Status, "Active", StringComparison.OrdinalIgnoreCase))
                        continue;

                    foreach (var item in createdRequest.Items)
                    {
                        var vendorProduct = await _vendorProductRepository.GetByVendorAndProductAsync(vendorId, item.ProductID);
                        if (vendorProduct == null || !string.Equals(vendorProduct.Status, "Active", StringComparison.OrdinalIgnoreCase))
                            continue;

                        var existingResp = await _opportunityResponseRepository.GetByRequestAndVendorAsync(
                            createdRequest.RequestID, vendorId, item.ProductID);

                        if (existingResp == null)
                        {
                            var opp = new VendorOpportunityResponse
                            {
                                RequestID = createdRequest.RequestID,
                                VendorID = vendorId,
                                ProductID = item.ProductID,
                                Status = "Pending",
                                CreatedDate = DateTime.UtcNow
                            };
                            await _opportunityResponseRepository.AddAsync(opp);
                        }

                        var vendorManagers = allUsers.Where(u =>
                            u.VendorID == vendorId &&
                            (string.Equals(u.Role?.RoleName, "Vendor Manager", StringComparison.OrdinalIgnoreCase) || u.RoleID == 4))
                            .ToList();

                        loadedProducts.TryGetValue(item.ProductID, out var prod);
                        string prodName = prod?.ProductName ?? $"Product #{item.ProductID}";

                        foreach (var vm in vendorManagers)
                        {
                            var notif = new Notification
                            {
                                UserID = vm.UserID,
                                Title = $"New Procurement Opportunity #PR-{createdRequest.RequestID}",
                                Message = $"You have received a new procurement opportunity for {item.Quantity:N2} {item.Unit} of {prodName} from {outletName}.",
                                NotificationType = "ProcurementOpportunity",
                                RelatedRequestID = createdRequest.RequestID,
                                RelatedVendorID = vendorId,
                                IsRead = false,
                                CreatedDate = DateTime.UtcNow
                            };
                            await _notificationRepository.AddAsync(notif);
                        }
                    }
                }
            }
        }

        var dto = new PurchaseRequestDto
        {
            RequestID = createdRequest.RequestID,
            OutletID = createdRequest.OutletID,
            CreatedByUserID = createdRequest.CreatedByUserID,
            RequestDate = createdRequest.RequestDate,
            Status = createdRequest.Status,
            Items = createdRequest.Items?.Select(i => new PurchaseRequestItemDto { RequestItemID = i.RequestItemID, RequestID = i.RequestID, ProductID = i.ProductID, ProductName = i.Product?.ProductName ?? (loadedProducts.TryGetValue(i.ProductID, out var p) ? p.ProductName : string.Empty), Quantity = i.Quantity, Unit = i.Unit }).ToList() ?? new List<PurchaseRequestItemDto>()
        };

        return new CreatePurchaseRequestResponse
        {
            PurchaseRequest = dto
        };
    }
}
