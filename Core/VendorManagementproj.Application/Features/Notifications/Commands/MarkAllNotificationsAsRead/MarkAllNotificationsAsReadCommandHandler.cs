using System.Threading;
using System.Threading.Tasks;
using MediatR;
using VendorManagementprojApplication.Contracts.Persistence;
using VendorManagementprojApplication.Contracts.Services;

namespace VendorManagementprojApplication.Features.Notifications.Commands.MarkAllNotificationsAsRead;

public class MarkAllNotificationsAsReadCommandHandler
    : IRequestHandler<MarkAllNotificationsAsReadCommand, MarkAllNotificationsAsReadResponse>
{
    private readonly INotificationRepository _notificationRepository;
    private readonly ICurrentUserService _currentUserService;

    public MarkAllNotificationsAsReadCommandHandler(
        INotificationRepository notificationRepository,
        ICurrentUserService currentUserService)
    {
        _notificationRepository = notificationRepository;
        _currentUserService = currentUserService;
    }

    public async Task<MarkAllNotificationsAsReadResponse> Handle(
        MarkAllNotificationsAsReadCommand request,
        CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserID;
        if (!userId.HasValue || userId.Value <= 0)
        {
            return new MarkAllNotificationsAsReadResponse { Success = false };
        }

        await _notificationRepository.MarkAllAsReadByUserIdAsync(userId.Value);

        return new MarkAllNotificationsAsReadResponse { Success = true };
    }
}
