using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VendorManagementprojApplication.Features.DeliveryRecords.Commands.ConfirmDeliveryRecord;
using VendorManagementprojApplication.Features.DeliveryRecords.Commands.CreateDeliveryRecord;
using VendorManagementprojApplication.Features.DeliveryRecords.Queries.GetDeliveriesByPurchaseOrder;
using VendorManagementprojApplication.Features.DeliveryRecords.Queries.GetSpoilageAdvice;

namespace VendorManagementprojApi.Controllers;

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
}
