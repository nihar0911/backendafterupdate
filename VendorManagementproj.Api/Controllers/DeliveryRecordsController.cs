using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VendorManagementproj.Application.Features.DeliveryRecords.Commands.ConfirmDeliveryRecord;
using VendorManagementproj.Application.Features.DeliveryRecords.Commands.CreateDeliveryRecord;
using VendorManagementproj.Application.Features.DeliveryRecords.Queries.GetDeliveriesByPurchaseOrder;
using VendorManagementproj.Application.Features.DeliveryRecords.Queries.GetMyDeliveries;
using VendorManagementproj.Application.Features.DeliveryRecords.Queries.GetSpoilageAdvice;

namespace VendorManagementproj.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class DeliveryRecordsController : ControllerBase
{
    private readonly IMediator _mediator;

    public DeliveryRecordsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost]
    [Authorize(Roles = "Admin,Purchase Manager")]
    public async Task<IActionResult> CreateDelivery(
        [FromBody] CreateDeliveryRecordCommand command)
    {
        var result =
            await _mediator.Send(command);

        return Ok(result);
    }

    [HttpGet("purchase-order/{purchaseOrderID}")]
    [Authorize(Roles = "Admin,Organization Manager,Vendor Manager,Outlet Manager,Purchase Manager")]
    public async Task<IActionResult> GetByPurchaseOrder(
        int purchaseOrderID)
    {
        var result =
            await _mediator.Send(
                new GetDeliveriesByPurchaseOrderQuery
                {
                    PurchaseOrderID = purchaseOrderID
                });

        return Ok(result);
    }

    [HttpPost("confirm")]
    [Authorize(Roles = "Admin,Purchase Manager")]
    public async Task<IActionResult> ConfirmDelivery(
        [FromBody] ConfirmDeliveryRecordCommand command)
    {
        var result =
            await _mediator.Send(command);

        return Ok(result);
    }

    [HttpGet("spoilage-advice/{purchaseOrderID:int}")]
    [Authorize(Roles = "Admin,Vendor Manager,Purchase Manager,Organization Manager,Outlet Manager")]
    public async Task<IActionResult> GetSpoilageAdvice(int purchaseOrderID)
    {
        try
        {
            var result = await _mediator.Send(new GetSpoilageAdviceQuery
            {
                PurchaseOrderID = purchaseOrderID
            });

            if (result.Advisor == null)
                return NotFound();

            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        } 
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(ex.Message);
        }
    }

    [HttpGet("vendor/my")]
    [HttpGet("my")]
    [Authorize(Roles = "Vendor Manager")]
    public async Task<IActionResult> GetMyDeliveries()
    {
        try
        {
            var result = await _mediator.Send(new GetMyDeliveriesQuery());
            return Ok(result);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}
