using System;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VendorManagementprojApplication.Features.Notifications.Commands.MarkNotificationAsRead;
using VendorManagementprojApplication.Features.Notifications.Queries.GetMyNotifications;

namespace VendorManagementprojApi.Controllers;

[ApiController]
[Route("api/notifications")]
[Authorize]
public class NotificationsController : ControllerBase
{
    private readonly IMediator _mediator;

    public NotificationsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<IActionResult> GetMyNotifications()
    {
        var response = await _mediator.Send(new GetMyNotificationsQuery());
        return Ok(response);
    }

    [HttpPut("{notificationID:int}/read")]
    public async Task<IActionResult> MarkAsRead(int notificationID)
    {
        try
        {
            var response = await _mediator.Send(new MarkNotificationAsReadCommand { NotificationID = notificationID });
            return Ok(response);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new { message = ex.Message });
        }
    }
}
