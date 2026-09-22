using System;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VendorManagementprojApplication.Features.VendorRecommendationSettings.Commands.UpdateVendorRecommendationSettings;
using VendorManagementprojApplication.Features.VendorRecommendationSettings.Queries.GetVendorRecommendationSettings;

namespace VendorManagementprojApi.Controllers;

[ApiController]
[Route("api/vendor-recommendation-settings")]
[Authorize]
public class VendorRecommendationSettingsController : ControllerBase
{
    private readonly IMediator _mediator;

    public VendorRecommendationSettingsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    [Authorize(Roles = "Admin,Organization Manager,Outlet Manager,Purchase Manager")]
    public async Task<IActionResult> GetSettings()
    {
        try
        {
            var response = await _mediator.Send(new GetVendorRecommendationSettingsQuery());
            return Ok(response.Settings);
        }
        catch (Exception)
        {
            return StatusCode(500, new { message = "An error occurred while retrieving vendor recommendation settings." });
        }
    }

    [HttpPut]
    [Authorize(Roles = "Admin,Organization Manager,Purchase Manager")]
    public async Task<IActionResult> UpdateSettings([FromBody] UpdateVendorRecommendationSettingsCommand command)
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
            return StatusCode(500, new { message = "An unexpected error occurred while saving vendor recommendation settings." });
        }
    }
}
