using System;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VendorManagementprojApplication.Features.SpoilageAdviceSettings.Commands.UpdateSpoilageAdviceSettings;
using VendorManagementprojApplication.Features.SpoilageAdviceSettings.Queries.GetSpoilageAdviceSettings;

namespace VendorManagementprojApi.Controllers;

[ApiController]
[Route("api/spoilage-advice-settings")]
[Authorize]
public class SpoilageAdviceSettingsController : ControllerBase
{
    private readonly IMediator _mediator;

    public SpoilageAdviceSettingsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    [Authorize(Roles = "Admin,Vendor Manager")]
    public async Task<IActionResult> GetSettings()
    {
        try
        {
            var response = await _mediator.Send(new GetSpoilageAdviceSettingsQuery());
            return Ok(response.Settings);
        }
        catch (Exception)
        {
            return StatusCode(500, new { message = "An error occurred while retrieving spoilage advice settings." });
        }
    }

    [HttpPut]
    [Authorize(Roles = "Admin,Vendor Manager")]
    public async Task<IActionResult> UpdateSettings([FromBody] UpdateSpoilageAdviceSettingsCommand command)
    {
        try
        {
            var response = await _mediator.Send(command);
            return Ok(response.Settings);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
        catch (Exception)
        {
            return StatusCode(500, new { message = "An unexpected error occurred while saving spoilage advice settings." });
        }
    }
}
