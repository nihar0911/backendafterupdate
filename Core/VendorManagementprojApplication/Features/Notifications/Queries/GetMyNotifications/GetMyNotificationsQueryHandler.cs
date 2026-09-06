using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using VendorManagementprojApplication.Contracts.Persistence;
using VendorManagementprojApplication.Contracts.Services;
using VendorManagementprojApplication.DTOs;

namespace VendorManagementprojApplication.Features.Notifications.Queries.GetMyNotifications;

public class GetMyNotificationsQueryHandler
    : IRequestHandler<GetMyNotificationsQuery, GetMyNotificationsResponse>
{
    private readonly INotificationRepository _notificationRepository;
    private readonly ICurrentUserService _currentUserService;

    public GetMyNotificationsQueryHandler(
        INotificationRepository notificationRepository,
        ICurrentUserService currentUserService)
    {
        _notificationRepository = notificationRepository;
        _currentUserService = currentUserService;
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

        var notifications = await _notificationRepository.GetByUserIdAsync(userId.Value);
        var unreadCount = await _notificationRepository.GetUnreadCountByUserIdAsync(userId.Value);

        var dtos = notifications.Select(n => new NotificationDto
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
