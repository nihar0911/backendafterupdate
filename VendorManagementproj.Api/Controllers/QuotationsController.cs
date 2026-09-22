using VendorManagementproj.Application.Features.Quotations.Queries.GetVendorQuotations;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VendorManagementproj.Application.Features.Quotations.Commands.AcceptQuotation;
using VendorManagementproj.Application.Features.Quotations.Commands.CreateQuotation;
using VendorManagementproj.Application.Features.Quotations.Commands.DeleteQuotation;
using VendorManagementproj.Application.Features.Quotations.Commands.RejectQuotation;
using VendorManagementproj.Application.Features.Quotations.Commands.RespondToQuotation;
using VendorManagementproj.Application.Features.Quotations.Commands.UpdateQuotation;
using VendorManagementproj.Application.Features.Quotations.Queries.GetAllQuotations;
using VendorManagementproj.Application.Features.Quotations.Queries.GetQuotationById;

namespace VendorManagementproj.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class QuotationsController : ControllerBase
{
    private readonly IMediator _mediator;

    public QuotationsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet("vendor/my")]
    [Authorize(Roles = "Vendor Manager")]
    public async Task<IActionResult> GetMyVendorQuotations()
    {
        try
        {
            var response = await _mediator.Send(new GetVendorQuotationsQuery());
            return Ok(response);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpGet]
    [Authorize(Roles = "Admin,Organization Manager,Outlet Manager,Vendor Manager,Purchase Manager")]
    public async Task<IActionResult> GetAll()
    {
        var response =
            await _mediator.Send(
                new GetAllQuotationsQuery());

        return Ok(response);
    }

    [HttpPost]
    [Authorize(Roles = "Vendor Manager")]
    public async Task<IActionResult> Create(
        CreateQuotationCommand command)
    {
        try
        {
            var response =
                await _mediator.Send(command);

            return CreatedAtAction(
                nameof(GetById),
                new
                {
                    quotationID =
                        response.Quotation.QuotationID
                },
                response);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPut("{quotationID:int}")]
    [Authorize(Roles = "Vendor Manager")]
    public async Task<IActionResult> Update(
        int quotationID,
        UpdateQuotationCommand command)
    {
        try
        {
            command.QuotationID = quotationID;

            var response =
                await _mediator.Send(command);

            if (response.Quotation == null)
                return NotFound();

            return Ok(response);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }
    [HttpPut("respond")]
    [Authorize(Roles = "Admin,Purchase Manager")]
    public async Task<IActionResult> Respond(
    RespondToQuotationCommand command)
    {
        try
        {
            var response =
                await _mediator.Send(command);

            return Ok(response);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPut("{quotationID:int}/accept")]
    [Authorize(Roles = "Admin,Purchase Manager")]
    public async Task<IActionResult> Accept(int quotationID)
    {
        try
        {
            var response = await _mediator.Send(new AcceptQuotationCommand { QuotationID = quotationID });
            return Ok(response);
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

    [HttpPut("{quotationID:int}/reject")]
    [Authorize(Roles = "Admin,Purchase Manager")]
    public async Task<IActionResult> Reject(int quotationID)
    {
        try
        {
            var response = await _mediator.Send(new RejectQuotationCommand { QuotationID = quotationID });
            return Ok(response);
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

    [HttpGet("{quotationID:int}")]
    [Authorize(Roles = "Admin,Organization Manager,Outlet Manager,Vendor Manager,Purchase Manager")]
    public async Task<IActionResult> GetById(
        int quotationID)
    {
        try
        {
            var response =
                await _mediator.Send(
                    new GetQuotationByIdQuery(quotationID));

            if (response.Quotation == null)
                return NotFound();

            return Ok(response);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(ex.Message);
        }
    }

    [HttpDelete("{quotationID:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(
        int quotationID)
    {
        var response =
            await _mediator.Send(
                new DeleteQuotationCommand(quotationID));

        if (!response.Success)
            return NotFound();

        return NoContent();
    }
}
