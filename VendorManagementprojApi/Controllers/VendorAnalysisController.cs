using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VendorManagementprojApplication.Features.VendorAnalysis.Queries.GetVendorAnalysis;

namespace VendorManagementprojApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class VendorAnalysisController : ControllerBase
{
    private readonly IMediator _mediator;

    public VendorAnalysisController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet("{vendorID:int}")]
    [Authorize(Roles = "Admin,Organization Manager,Outlet Manager")]
    public async Task<IActionResult> GetAnalysis(
        int vendorID,
        [FromQuery] int productID,
        [FromQuery] int outletID)
    {
        try
        {
            var response = await _mediator.Send(
                new GetVendorAnalysisQuery
                {
                    VendorID = vendorID,
                    ProductID = productID,
                    OutletID = outletID
                });

            if (response.Analysis == null)
                return NotFound();

            return Ok(response);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }
}