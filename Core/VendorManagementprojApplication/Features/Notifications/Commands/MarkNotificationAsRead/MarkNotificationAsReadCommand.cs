using MediatR;

namespace VendorManagementprojApplication.Features.Notifications.Commands.MarkNotificationAsRead;

public class MarkNotificationAsReadCommand : IRequest<MarkNotificationAsReadResponse>
{
    public int NotificationID { get; set; }
}
