using System.Collections.Generic;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VendorManagementprojApplication.Features.PurchaseRequests.Commands.AddPurchaseRequestItem;
using VendorManagementprojApplication.Features.PurchaseRequests.Commands.CreatePurchaseRequest;
using VendorManagementprojApplication.Features.PurchaseRequests.Commands.DeletePurchaseRequest;
using VendorManagementprojApplication.Features.PurchaseRequests.Commands.DeletePurchaseRequestItem;
using VendorManagementprojApplication.Features.PurchaseRequests.Commands.DispatchPurchaseRequest;
using VendorManagementprojApplication.Features.PurchaseRequests.Commands.RespondToOpportunity;
using VendorManagementprojApplication.Features.PurchaseRequests.Commands.UpdatePurchaseRequest;
using VendorManagementprojApplication.Features.PurchaseRequests.Queries.GetAllPurchaseRequests;
using VendorManagementprojApplication.Features.PurchaseRequests.Queries.GetPurchaseRequestById;
using VendorManagementprojApplication.Features.PurchaseRequests.Queries.GetPurchaseRequestItems;
using VendorManagementprojApplication.Features.PurchaseRequests.Queries.GetVendorProcurementOpportunities;

namespace VendorManagementprojApi.Controllers;

[ApiController]
[Route("api/purchaserequests")]
[Authorize]
public class PurchaseRequestsController : ControllerBase
{
    private readonly IMediator _mediator;

    public PurchaseRequestsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet("vendor/opportunities")]
    [Authorize(Roles = "Vendor Manager")]
    public async Task<IActionResult> GetVendorOpportunities()
    {
        try
        {
            var response = await _mediator.Send(new GetVendorProcurementOpportunitiesQuery());
            return Ok(response);
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

    [HttpPost("vendor/opportunities/respond")]
    [Authorize(Roles = "Vendor Manager")]
    public async Task<IActionResult> RespondToOpportunity(RespondToOpportunityCommand command)
    {
        try
        {
            var response = await _mediator.Send(command);
            return Ok(response);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new { message = ex.Message });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("dispatch")]
    [HttpPost("{requestID:int}/dispatch")]
    [Authorize(Roles = "Admin,Purchase Manager")]
    public async Task<IActionResult> DispatchToSelectedVendors(int? requestID, [FromBody] DispatchPurchaseRequestCommand? command)
    {
        try
        {
            var cmd = command ?? new DispatchPurchaseRequestCommand();
            if (requestID.HasValue && requestID.Value > 0)
            {
                cmd.RequestID = requestID.Value;
            }

            var response = await _mediator.Send(cmd);
            return Ok(response);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new { message = ex.Message });
        }
    }

    [HttpGet]
    [Authorize(Roles = "Admin,Organization Manager,Outlet Manager,Purchase Manager")]
    public async Task<IActionResult> GetAll()
    {
        var response = await _mediator.Send(new GetAllPurchaseRequestsQuery());
        return Ok(response);
    }

    [HttpGet("{requestID:int}")]
    [Authorize(Roles = "Admin,Organization Manager,Outlet Manager,Purchase Manager")]
    public async Task<IActionResult> GetById(int requestID)
    {
        try
        {
            var response = await _mediator.Send(new GetPurchaseRequestByIdQuery(requestID));
            if (response.PurchaseRequest == null)
                return NotFound();

            return Ok(response);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new { message = ex.Message });
        }
    }

    [HttpPost]
    [Authorize(Roles = "Admin,Purchase Manager")]
    public async Task<IActionResult> Create(CreatePurchaseRequestCommand command)
    {
        try
        {
            var response = await _mediator.Send(command);
            return CreatedAtAction(
                nameof(GetById),
                new { requestID = response.PurchaseRequest.RequestID },
                response);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new { message = ex.Message });
        }
    }

    [HttpDelete("{requestID:int}")]
    [ApiExplorerSettings(IgnoreApi = true)]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int requestID)
    {
        var response = await _mediator.Send(new DeletePurchaseRequestCommand(requestID));
        if (!response.Success)
            return NotFound();

        return NoContent();
    }
    
    [HttpGet("{requestID:int}/items")]
    [ApiExplorerSettings(IgnoreApi = true)]
    [Authorize(Roles = "Admin,Organization Manager,Outlet Manager,Purchase Manager")]
    public async Task<IActionResult> GetItems(int requestID)
    {
        try
        {
            var response = await _mediator.Send(new GetPurchaseRequestItemsQuery(requestID));
            return Ok(response);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new { message = ex.Message });
        }
    }

    [HttpPost("{requestID:int}/items")]
    [Authorize(Roles = "Admin,Purchase Manager")]
    public async Task<IActionResult> AddItem(int requestID, AddPurchaseRequestItemCommand command)
    {
        try
        {
            command.RequestID = requestID;
            var response = await _mediator.Send(command);
            if (response.Item == null)
                return NotFound(new { message = "Purchase Request not found." });

            return Ok(response);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new { message = ex.Message });
        }
    }

    [HttpPut("{requestID:int}")]
    [Authorize(Roles = "Admin,Purchase Manager")]
    public async Task<IActionResult> Update(int requestID, UpdatePurchaseRequestCommand command)
    {
        try
        {
            command.RequestID = requestID;
            var response = await _mediator.Send(command);
            if (response.PurchaseRequest == null)
                return NotFound();

            return Ok(response);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new { message = ex.Message });
        }
    }

    [HttpDelete("items/{itemID:int}")]
    [Authorize(Roles = "Admin,Purchase Manager")]
    public async Task<IActionResult> DeleteItem(int itemID)
    {
        try
        {
            var response = await _mediator.Send(new DeletePurchaseRequestItemCommand(itemID));
            if (!response.Success)
                return NotFound();

            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new { message = ex.Message });
        }
    }
}
