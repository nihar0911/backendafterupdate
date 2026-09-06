using System.Threading;
using System.Threading.Tasks;
using MediatR;
using VendorManagementprojApplication.Contracts.Persistence;
using VendorManagementprojApplication.Contracts.Services;

namespace VendorManagementprojApplication.Features.Notifications.Commands.MarkNotificationAsRead;

public class MarkNotificationAsReadCommandHandler
    : IRequestHandler<MarkNotificationAsReadCommand, MarkNotificationAsReadResponse>
{
    private readonly INotificationRepository _notificationRepository;
    private readonly ICurrentUserService _currentUserService;

    public MarkNotificationAsReadCommandHandler(
        INotificationRepository notificationRepository,
        ICurrentUserService currentUserService)
    {
        _notificationRepository = notificationRepository;
        _currentUserService = currentUserService;
    }

    public async Task<MarkNotificationAsReadResponse> Handle(
        MarkNotificationAsReadCommand request,
        CancellationToken cancellationToken)
    {
        var notification = await _notificationRepository.GetByIdAsync(request.NotificationID);
        if (notification == null)
        {
            return new MarkNotificationAsReadResponse { Success = false };
        }

        if (_currentUserService.UserID.HasValue && notification.UserID != _currentUserService.UserID.Value)
        {
            throw new UnauthorizedAccessException("Cannot modify notifications for another user.");
        }

        notification.IsRead = true;
        await _notificationRepository.UpdateAsync(notification);

        return new MarkNotificationAsReadResponse { Success = true };
    }
}
