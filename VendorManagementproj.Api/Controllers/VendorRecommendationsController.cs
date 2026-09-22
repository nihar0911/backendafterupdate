using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VendorManagementproj.Application.Features.VendorRecommendations.Queries.GetRecommendations;
using VendorManagementproj.Application.Features.VendorRecommendations.Queries.GetRecommendationsForProduct;

namespace VendorManagementproj.Api.Controllers;

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
                new GetRecommendationsQuery
                {
                    PurchaseRequestID = purchaseRequestID
                });

            return Ok(response);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (UnauthorizedAccessException ex)
        {
            return StatusCode(403, new { message = ex.Message });
        }
    }

    [HttpGet("product/{productID:int}")]
    [Authorize(Roles = "Admin,Organization Manager,Outlet Manager,Purchase Manager")]
    public async Task<IActionResult> GetRecommendationsForProduct(
        int productID,
        [FromQuery] int outletID)
    {
        try
        {
            var response = await _mediator.Send(
                new GetRecommendationsForProductQuery(outletID, productID));

            return Ok(response);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (UnauthorizedAccessException ex)
        {
            return StatusCode(403, new { message = ex.Message });
        }
    }

    [HttpGet("outlet/{outletID:int}/product/{productID:int}")]
    [Authorize(Roles = "Admin,Organization Manager,Outlet Manager,Purchase Manager")]
    public async Task<IActionResult> GetRecommendationsByOutletAndProduct(
        int outletID,
        int productID)
    {
        try
        {
            var response = await _mediator.Send(
                new GetRecommendationsForProductQuery(outletID, productID));

            return Ok(response);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (UnauthorizedAccessException ex)
        {
            return StatusCode(403, new { message = ex.Message });
        }
    }
}
