using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using VendorManagementproj.Application.Contracts.Persistence;
using VendorManagementproj.Application.Contracts.Services;
using VendorManagementproj.Application.DTOs;
using VendorManagementproj.Domain.Entities;

namespace VendorManagementproj.Application.Features.Notifications.Queries.GetMyNotifications;

public class GetMyNotificationsQueryHandler
    : IRequestHandler<GetMyNotificationsQuery, GetMyNotificationsResponse>
{
    private readonly INotificationRepository _notificationRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly IUserRepository _userRepository;
    private readonly IPurchaseOrderRepository _purchaseOrderRepository;
    private readonly IPurchaseRequestRepository _purchaseRequestRepository;
    private readonly IOutletRepository _outletRepository;
    private readonly IVendorFeedbackRepository _vendorFeedbackRepository;

    public GetMyNotificationsQueryHandler(
        INotificationRepository notificationRepository,
        ICurrentUserService currentUserService,
        IUserRepository userRepository,
        IPurchaseOrderRepository purchaseOrderRepository,
        IPurchaseRequestRepository purchaseRequestRepository,
        IOutletRepository outletRepository,
        IVendorFeedbackRepository vendorFeedbackRepository)
    {
        _notificationRepository = notificationRepository;
        _currentUserService = currentUserService;
        _userRepository = userRepository;
        _purchaseOrderRepository = purchaseOrderRepository;
        _purchaseRequestRepository = purchaseRequestRepository;
        _outletRepository = outletRepository;
        _vendorFeedbackRepository = vendorFeedbackRepository;
    }

    public async Task<GetMyNotificationsResponse> Handle(
        GetMyNotificationsQuery request,
        CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserID;
        if (!userId.HasValue || userId.Value <= 0)
        {
            return new GetMyNotificationsResponse();
        }

        var currentUser = await _userRepository.GetByIdAsync(userId.Value);
        var roleName = currentUser?.Role?.RoleName ?? _currentUserService.Role ?? string.Empty;
        int? userOutletId = currentUser?.OutletID ?? _currentUserService.OutletID;
        int? userOrgId = currentUser?.OrganizationID ?? _currentUserService.OrganizationID;
        int? userVendorId = currentUser?.VendorID ?? _currentUserService.VendorID;

        var rawNotifications = await _notificationRepository.GetByUserIdAsync(userId.Value);
        var scopedNotifications = new List<Notification>();

        var poCache = new Dictionary<int, PurchaseOrder?>();
        var prCache = new Dictionary<int, PurchaseRequest?>();
        var outletCache = new Dictionary<int, Outlet?>();

        foreach (var n in rawNotifications)
        {
            bool isRelevant = true;

            // Purchase Manager & Outlet Manager: Strict Outlet-level source-entity scoping
            if (string.Equals(roleName, "Purchase Manager", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(roleName, "Outlet Manager", StringComparison.OrdinalIgnoreCase))
            {
                if (userOutletId.HasValue)
                {
                    if (n.RelatedRequestID.HasValue && n.RelatedRequestID.Value > 0)
                    {
                        int reqId = n.RelatedRequestID.Value;

                        // Check if it corresponds to a Purchase Order
                        if (!poCache.TryGetValue(reqId, out var po))
                        {
                            po = await _purchaseOrderRepository.GetByIdAsync(reqId);
                            poCache[reqId] = po;
                        }

                        if (po != null)
                        {
                            if (po.OutletID != userOutletId.Value)
                            {
                                isRelevant = false;
                            }
                        }
                        else
                        {
                            // Check if it corresponds to a Purchase Request
                            if (!prCache.TryGetValue(reqId, out var pr))
                            {
                                pr = await _purchaseRequestRepository.GetByIdAsync(reqId);
                                prCache[reqId] = pr;
                            }

                            if (pr != null)
                            {
                                if (pr.OutletID != userOutletId.Value)
                                {
                                    isRelevant = false;
                                }
                            }
                        }
                    }
                }
            }
            // Organization Manager: Strict Organization-level scoping
            else if (string.Equals(roleName, "Organization Manager", StringComparison.OrdinalIgnoreCase))
            {
                if (userOrgId.HasValue && n.RelatedRequestID.HasValue && n.RelatedRequestID.Value > 0)
                {
                    int reqId = n.RelatedRequestID.Value;

                    if (!poCache.TryGetValue(reqId, out var po))
                    {
                        po = await _purchaseOrderRepository.GetByIdAsync(reqId);
                        poCache[reqId] = po;
                    }

                    if (po != null)
                    {
                        if (!outletCache.TryGetValue(po.OutletID, out var outlet))
                        {
                            outlet = await _outletRepository.GetByIdAsync(po.OutletID);
                            outletCache[po.OutletID] = outlet;
                        }

                        if (outlet != null && outlet.OrganizationID != userOrgId.Value)
                        {
                            isRelevant = false;
                        }
                    }
                    else
                    {
                        if (!prCache.TryGetValue(reqId, out var pr))
                        {
                            pr = await _purchaseRequestRepository.GetByIdAsync(reqId);
                            prCache[reqId] = pr;
                        }

                        if (pr != null)
                        {
                            if (!outletCache.TryGetValue(pr.OutletID, out var outlet))
                            {
                                outlet = await _outletRepository.GetByIdAsync(pr.OutletID);
                                outletCache[pr.OutletID] = outlet;
                            }

                            if (outlet != null && outlet.OrganizationID != userOrgId.Value)
                            {
                                isRelevant = false;
                            }
                        }
                    }
                }
            }
            // Vendor Manager: Strict Vendor-level scoping
            else if (string.Equals(roleName, "Vendor Manager", StringComparison.OrdinalIgnoreCase))
            {
                if (userVendorId.HasValue)
                {
                    if (n.RelatedVendorID.HasValue && n.RelatedVendorID.Value != userVendorId.Value)
                    {
                        isRelevant = false;
                    }
                }
            }

            if (isRelevant)
            {
                scopedNotifications.Add(n);
            }
        }

        var unreadCount = scopedNotifications.Count(n => !n.IsRead);

        var dtos = scopedNotifications.Select(n => new NotificationDto
        {
            NotificationID = n.NotificationID,
            UserID = n.UserID,
            RelatedRequestID = n.RelatedRequestID,
            RelatedVendorID = n.RelatedVendorID,
            Title = n.Title,
            Message = n.Message,
            NotificationType = n.NotificationType,
            IsRead = n.IsRead,
            CreatedDate = n.CreatedDate
        }).OrderByDescending(n => n.CreatedDate).ToList();

        return new GetMyNotificationsResponse
        {
            Notifications = dtos,
            UnreadCount = unreadCount
        };
    }
}
