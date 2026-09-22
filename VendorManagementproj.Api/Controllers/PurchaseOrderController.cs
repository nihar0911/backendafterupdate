using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VendorManagementproj.Application.Features.PurchaseOrders.Commands.ApprovePurchaseOrder;
using VendorManagementproj.Application.Features.PurchaseOrders.Commands.ChangePurchaseOrderApproverRole;
using VendorManagementproj.Application.Features.PurchaseOrders.Commands.CreatePurchaseOrder;
using VendorManagementproj.Application.Features.PurchaseOrders.Commands.DispatchPurchaseOrder;
using VendorManagementproj.Application.Features.PurchaseOrders.Commands.RejectPurchaseOrder;
using VendorManagementproj.Application.Features.PurchaseOrders.Commands.RespondToPurchaseOrder;
using VendorManagementproj.Application.Features.PurchaseOrders.Commands.SendPurchaseOrder;
using VendorManagementproj.Application.Features.PurchaseOrders.Queries.GetAllPurchaseOrders;
using VendorManagementproj.Application.Features.PurchaseOrders.Queries.GetPendingPurchaseOrders;
using VendorManagementproj.Application.Features.PurchaseOrders.Queries.GetPurchaseOrderById;

namespace VendorManagementproj.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PurchaseOrderController : ControllerBase
{
    private readonly IMediator _mediator;

    public PurchaseOrderController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost]
    [Authorize(Roles = "Admin,Purchase Manager")]
    public async Task<IActionResult> Create(
        [FromBody] CreatePurchaseOrderCommand command)
    {
        try
        {
            var result = await _mediator.Send(command);
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

    [HttpPut("{purchaseOrderID:int}/approve")]
    [Authorize(Roles = "Organization Manager,Outlet Manager,Admin")]
    public async Task<IActionResult> Approve(int purchaseOrderID)
    {
        try
        {
            var result = await _mediator.Send( new ApprovePurchaseOrderCommand
                {
                     PurchaseOrderID = purchaseOrderID
                });

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

    [HttpPut("{purchaseOrderID:int}/reject")]
    [Authorize(Roles = "Organization Manager,Outlet Manager,Admin")]
    public async Task<IActionResult> Reject(int purchaseOrderID)
    {
        try
        {
            var result = await _mediator.Send(
                new RejectPurchaseOrderCommand
                {
                    PurchaseOrderID = purchaseOrderID
                });

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

    [HttpPost("{purchaseOrderID:int}/send")]
    [Authorize(Roles = "Purchase Manager")]
    public async Task<IActionResult> Send(int purchaseOrderID)
    {
        try
        {
            var result = await _mediator.Send(
                new SendPurchaseOrderCommand
                {
                    PurchaseOrderID = purchaseOrderID
                });

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

    [HttpPut("{purchaseOrderID:int}/approver-role")]
    [Authorize(Roles = "Admin,Purchase Manager")]
    public async Task<IActionResult> ChangeApproverRole(
        int purchaseOrderID,
        [FromBody] ChangePurchaseOrderApproverRoleCommand command)
    {
        try
        {
            command.PurchaseOrderID = purchaseOrderID;

            var result = await _mediator.Send(command);

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

    [HttpPut("respond")]
    [Authorize(Roles = "Admin,Vendor Manager")]
    public async Task<IActionResult> Respond(
        [FromBody] RespondToPurchaseOrderCommand command)
    {
        var result = await _mediator.Send(command);

        return Ok(result);
    }

    [HttpGet("vendor/{vendorID}/pending")]
    [Authorize(Roles = "Admin,Vendor Manager")]
    public async Task<IActionResult> GetPendingByVendor(
        int vendorID)
    {
        var result =
            await _mediator.Send(
                new GetPendingPurchaseOrdersQuery
                {
                    VendorID = vendorID
                });

        return Ok(result);
    }

    [HttpGet]
    [Authorize(Roles = "Admin,Organization Manager,Vendor Manager,Outlet Manager,Purchase Manager")]
    public async Task<IActionResult> GetAll()
    {
        var result =
            await _mediator.Send(
                new GetAllPurchaseOrdersQuery());

        return Ok(result);
    }

    [HttpGet("{purchaseOrderID}")]
    [Authorize(Roles = "Admin,Organization Manager,Vendor Manager,Outlet Manager,Purchase Manager")]
    public async Task<IActionResult> GetById(
        int purchaseOrderID)
    {
        try
        {
            var result =
                await _mediator.Send(
                    new GetPurchaseOrderByIdQuery
                    {
                        PurchaseOrderID = purchaseOrderID
                    });

            if (result.PurchaseOrder == null)
                return NotFound();

            return Ok(result);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(ex.Message);
        }
    }

    [HttpPost("dispatch")]
    [Authorize(Roles = "Admin,Vendor Manager")]
    public async Task<IActionResult> Dispatch(
        [FromBody] DispatchPurchaseOrderCommand command)
    {
        var result =
            await _mediator.Send(command);

        return Ok(result);
    }
}
