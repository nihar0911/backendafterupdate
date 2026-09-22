using System.Threading;
using System.Threading.Tasks;
using MediatR;
using VendorManagementproj.Application.Contracts.Persistence;
using VendorManagementproj.Application.Contracts.Services;

namespace VendorManagementproj.Application.Features.Notifications.Commands.ClearAllNotifications;

public class ClearAllNotificationsCommandHandler
    : IRequestHandler<ClearAllNotificationsCommand, ClearAllNotificationsResponse>
{
    private readonly INotificationRepository _notificationRepository;
    private readonly ICurrentUserService _currentUserService;

    public ClearAllNotificationsCommandHandler(
        INotificationRepository notificationRepository,
        ICurrentUserService currentUserService)
    {
        _notificationRepository = notificationRepository;
        _currentUserService = currentUserService;
    }

    public async Task<ClearAllNotificationsResponse> Handle(
        ClearAllNotificationsCommand request,
        CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserID;
        if (!userId.HasValue || userId.Value <= 0)
        {
            return new ClearAllNotificationsResponse { Success = false };
        }

        await _notificationRepository.ClearAllByUserIdAsync(userId.Value);

        return new ClearAllNotificationsResponse { Success = true };
    }
}
