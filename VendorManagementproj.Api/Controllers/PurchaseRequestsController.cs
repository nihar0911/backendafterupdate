using System.Collections.Generic;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VendorManagementproj.Application.Features.PurchaseRequests.Commands.AddPurchaseRequestItem;
using VendorManagementproj.Application.Features.PurchaseRequests.Commands.CreatePurchaseRequest;
using VendorManagementproj.Application.Features.PurchaseRequests.Commands.DeletePurchaseRequest;
using VendorManagementproj.Application.Features.PurchaseRequests.Commands.DeletePurchaseRequestItem;
using VendorManagementproj.Application.Features.PurchaseRequests.Commands.DispatchPurchaseRequest;
using VendorManagementproj.Application.Features.PurchaseRequests.Commands.RespondToOpportunity;
using VendorManagementproj.Application.Features.PurchaseRequests.Commands.UpdatePurchaseRequest;
using VendorManagementproj.Application.Features.PurchaseRequests.Queries.GetAllPurchaseRequests;
using VendorManagementproj.Application.Features.PurchaseRequests.Queries.GetPurchaseRequestById;
using VendorManagementproj.Application.Features.PurchaseRequests.Queries.GetPurchaseRequestItems;
using VendorManagementproj.Application.Features.PurchaseRequests.Queries.GetVendorProcurementOpportunities;
using VendorManagementproj.Application.Features.PurchaseRequests.Queries.ParseVoiceProcurementOrder;

namespace VendorManagementproj.Api.Controllers;

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

    [HttpPost("ai-parse-order")]
    [Authorize(Roles = "Admin,Purchase Manager")]
    public async Task<IActionResult> AiParseOrder([FromBody] ParseVoiceProcurementOrderQuery  query)
    {
        try
        {
            if (query == null || string.IsNullOrWhiteSpace(query.Prompt))
            {
                return BadRequest(new { message = "Prompt is required." });
            }

            var response = await _mediator.Send(query);
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
