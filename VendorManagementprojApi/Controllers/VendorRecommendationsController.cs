using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VendorManagementprojApplication.Features.VendorRecommendations.Queries.GetRecommendations;

namespace VendorManagementprojApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class VendorRecommendationsController : ControllerBase
{
    private readonly IMediator _mediator;

    public VendorRecommendationsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet("{purchaseRequestID:int}")]
    [Authorize(Roles = "Admin,Organization Manager,Outlet Manager,Purchase Manager")]
    public async Task<IActionResult> GetRecommendations(
        int purchaseRequestID)
    {
        try
        {
            var response = await _mediator.Send(
                new Query
                {
                    PurchaseRequestID = purchaseRequestID
                });

            return Ok(response);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new { message = ex.Message });
        }
    }
}