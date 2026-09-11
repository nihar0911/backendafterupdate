using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using VendorManagementprojApplication.Contracts.Persistence;
using VendorManagementprojApplication.Contracts.Services;
using VendorManagementprojDomain.Entities;

namespace VendorManagementprojApplication.Features.PurchaseRequests.Commands.RespondToOpportunity;

public class RespondToOpportunityCommandHandler
    : IRequestHandler<RespondToOpportunityCommand, RespondToOpportunityResponse>
{
    private readonly IVendorOpportunityResponseRepository _responseRepository;
    private readonly IPurchaseRequestRepository _purchaseRequestRepository;
    private readonly IVendorProductRepository _vendorProductRepository;
    private readonly IVendorRepository _vendorRepository;
    private readonly IProductRepository _productRepository;
    private readonly IOutletRepository _outletRepository;
    private readonly IUserRepository _userRepository;
    private readonly INotificationRepository _notificationRepository;
    private readonly ICurrentUserService _currentUserService;

    public RespondToOpportunityCommandHandler(
        IVendorOpportunityResponseRepository responseRepository,
        IPurchaseRequestRepository purchaseRequestRepository,
        IVendorProductRepository vendorProductRepository,
        IVendorRepository vendorRepository,
        IProductRepository productRepository,
        IOutletRepository outletRepository,
        IUserRepository userRepository,
        INotificationRepository notificationRepository,
        ICurrentUserService currentUserService)
    {
        _responseRepository = responseRepository;
        _purchaseRequestRepository = purchaseRequestRepository;
        _vendorProductRepository = vendorProductRepository;
        _vendorRepository = vendorRepository;
        _productRepository = productRepository;
        _outletRepository = outletRepository;
        _userRepository = userRepository;
        _notificationRepository = notificationRepository;
        _currentUserService = currentUserService;
    }

    public async Task<RespondToOpportunityResponse> Handle(
        RespondToOpportunityCommand request,
        CancellationToken cancellationToken)
    {
        if (!_currentUserService.IsVendorManager || !_currentUserService.VendorID.HasValue)
        {
            throw new UnauthorizedAccessException("Only authorized Vendor Managers can respond to procurement opportunities.");
        }

        int vendorId = _currentUserService.VendorID.Value;

        var vendor = await _vendorRepository.GetByIdAsync(vendorId);
        if (vendor == null)
        {
            throw new KeyNotFoundException($"Vendor #{vendorId} not found.");
        }

        var pr = await _purchaseRequestRepository.GetByIdAsync(request.RequestID);
        if (pr == null)
        {
            throw new KeyNotFoundException($"Purchase Request #{request.RequestID} not found.");
        }

        // Verify product in PR
        var prItem = request.ProductID > 0
            ? pr.Items.FirstOrDefault(i => i.ProductID == request.ProductID)
            : (pr.Items.Count == 1 ? pr.Items.First() : null);

        if (prItem == null)
        {
            throw new InvalidOperationException(
                request.ProductID > 0
                    ? $"Product #{request.ProductID} is not part of Purchase Request #{request.RequestID}."
                    : $"Purchase Request #{request.RequestID} contains multiple products. ProductID must be specified.");
        }

        int productId = prItem.ProductID;

        // Verify active vendor product mapping
        var mapping = await _vendorProductRepository.GetByVendorAndProductAsync(vendorId, productId);
        if (mapping == null || !string.Equals(mapping.Status, "Active", StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException($"Vendor does not have an active product mapping for Product #{productId}.");
        }

        string action = request.Action?.Trim() ?? string.Empty;
        bool isAccept = string.Equals(action, "Accept", StringComparison.OrdinalIgnoreCase) ||
                        string.Equals(action, "Accepted", StringComparison.OrdinalIgnoreCase);
        bool isReject = string.Equals(action, "Reject", StringComparison.OrdinalIgnoreCase) ||
                        string.Equals(action, "Rejected", StringComparison.OrdinalIgnoreCase);

        if (!isAccept && !isReject)
        {
            throw new InvalidOperationException("Action must be either 'Accept' or 'Reject'.");
        }

        string newStatus = isAccept ? "Accepted" : "Rejected";

        var existingResponse = await _responseRepository.GetByRequestAndVendorAsync(request.RequestID, vendorId, productId);
        if (existingResponse == null)
        {
            existingResponse = new VendorOpportunityResponse
            {
                RequestID = request.RequestID,
                VendorID = vendorId,
                ProductID = productId,
                Status = newStatus,
                RejectionReason = isReject ? request.RejectionReason : null,
                ResponseDate = DateTime.UtcNow,
                CreatedDate = DateTime.UtcNow
            };
            await _responseRepository.AddAsync(existingResponse);
        }
        else
        {
            existingResponse.Status = newStatus;
            existingResponse.RejectionReason = isReject ? request.RejectionReason : null;
            existingResponse.ResponseDate = DateTime.UtcNow;
            await _responseRepository.UpdateAsync(existingResponse);
        }

        // Send notifications to the assigned Outlet Manager and PR Creator
        try
        {
            var targetOutlet = await _outletRepository.GetByIdAsync(pr.OutletID);
            string outletName = !string.IsNullOrWhiteSpace(targetOutlet?.OutletName) ? targetOutlet.OutletName : $"Outlet #{pr.OutletID}";
            var allUsers = await _userRepository.GetAllAsync();

            // Find the Outlet Manager(s) assigned to this specific outlet, or for this organization
            var outletManagers = allUsers.Where(u =>
                u.OutletID == pr.OutletID &&
                (string.Equals(u.Role?.RoleName, "Outlet Manager", StringComparison.OrdinalIgnoreCase) || u.RoleID == 3))
                .ToList();

            if (outletManagers.Count == 0 && targetOutlet != null)
            {
                outletManagers = allUsers.Where(u =>
                    u.OrganizationID == targetOutlet.OrganizationID &&
                    (string.Equals(u.Role?.RoleName, "Outlet Manager", StringComparison.OrdinalIgnoreCase) || u.RoleID == 3))
                    .ToList();
            }

            var notifiedUserIds = new HashSet<int>();

            if (isAccept)
            {
                // 1. Notify the specific Outlet Manager(s) assigned to this outlet
                foreach (var om in outletManagers)
                {
                    if (notifiedUserIds.Add(om.UserID))
                    {
                        var notif = new Notification
                        {
                            UserID = om.UserID,
                            RelatedRequestID = pr.RequestID,
                            RelatedVendorID = vendorId,
                            Title = $"Purchase Request PR-{pr.RequestID} Accepted",
                            Message = $"Purchase Request PR-{pr.RequestID} for {outletName} has been accepted by {vendor.VendorName}.",
                            NotificationType = "OpportunityAccepted",
                            IsRead = false,
                            CreatedDate = DateTime.UtcNow
                        };
                        await _notificationRepository.AddAsync(notif);
                    }
                }

                
                if (pr.CreatedByUserID > 0 && notifiedUserIds.Add(pr.CreatedByUserID))
                {
                    var creatorNotif = new Notification
                    {
                        UserID = pr.CreatedByUserID,
                        RelatedRequestID = pr.RequestID,
                        RelatedVendorID = vendorId,
                        Title = $"Purchase Request PR-{pr.RequestID} Accepted",
                        Message = $"Purchase Request PR-{pr.RequestID} for {outletName} has been accepted by {vendor.VendorName}.",
                        NotificationType = "OpportunityAccepted",
                        IsRead = false,
                        CreatedDate = DateTime.UtcNow
                    };
                    await _notificationRepository.AddAsync(creatorNotif);
                }
            }
            else if (isReject)
            {
                string reasonText = !string.IsNullOrWhiteSpace(request.RejectionReason)
                    ? $" Reason: {request.RejectionReason.Trim()}"
                    : string.Empty;

                // 1. Notify the specific Outlet Manager(s) assigned to this outlet
                foreach (var om in outletManagers)
                {
                    if (notifiedUserIds.Add(om.UserID))
                    {
                        var notif = new Notification
                        {
                            UserID = om.UserID,
                            RelatedRequestID = pr.RequestID,
                            RelatedVendorID = vendorId,
                            Title = $"Purchase Request PR-{pr.RequestID} Rejected",
                            Message = $"Purchase Request PR-{pr.RequestID} for {outletName} has been rejected by {vendor.VendorName}.{reasonText}",
                            NotificationType = "OpportunityRejected",
                            IsRead = false,
                            CreatedDate = DateTime.UtcNow
                        };
                        await _notificationRepository.AddAsync(notif);
                    }
                }

              
                if (pr.CreatedByUserID > 0 && notifiedUserIds.Add(pr.CreatedByUserID))
                {
                    var creatorNotif = new Notification
                    {
                        UserID = pr.CreatedByUserID,
                        RelatedRequestID = pr.RequestID,
                        RelatedVendorID = vendorId,
                        Title = $"Purchase Request PR-{pr.RequestID} Rejected",
                        Message = $"Purchase Request PR-{pr.RequestID} for {outletName} has been rejected by {vendor.VendorName}.{reasonText}",
                        NotificationType = "OpportunityRejected",
                        IsRead = false,
                        CreatedDate = DateTime.UtcNow
                    };
                    await _notificationRepository.AddAsync(creatorNotif);
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[RespondToOpportunity] Notification creation error: {ex.Message}");
        }

        return new RespondToOpportunityResponse
        {
            Success = true,
            Message = isAccept ? "Purchase Request accepted." : "Purchase Request rejected.",
            OpportunityStatus = newStatus,
            RequestID = request.RequestID,
            VendorID = vendorId,
            ProductID = productId
        };
    }
}
