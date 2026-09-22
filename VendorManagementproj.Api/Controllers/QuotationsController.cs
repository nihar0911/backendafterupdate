using VendorManagementprojApplication.Features.Quotations.Queries.GetVendorQuotations;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VendorManagementprojApplication.Features.Quotations.Commands.AcceptQuotation;
using VendorManagementprojApplication.Features.Quotations.Commands.CreateQuotation;
using VendorManagementprojApplication.Features.Quotations.Commands.DeleteQuotation;
using VendorManagementprojApplication.Features.Quotations.Commands.RejectQuotation;
using VendorManagementprojApplication.Features.Quotations.Commands.RespondToQuotation;
using VendorManagementprojApplication.Features.Quotations.Commands.UpdateQuotation;
using VendorManagementprojApplication.Features.Quotations.Queries.GetAllQuotations;
using VendorManagementprojApplication.Features.Quotations.Queries.GetQuotationById;

namespace VendorManagementprojApi.Controllers;

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
