using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VendorManagementproj.Application.Features.TaxRates.Commands.CreateTaxRate;
using VendorManagementproj.Application.Features.TaxRates.Commands.DeleteTaxRate;
using VendorManagementproj.Application.Features.TaxRates.Commands.UpdateTaxRate;
using VendorManagementproj.Application.Features.TaxRates.Queries.GetAllTaxRates;
using VendorManagementproj.Application.Features.TaxRates.Queries.GetTaxRateById;

namespace VendorManagementproj.Api.Controllers;

[ApiController]
[Route("api/taxrates")]
[Authorize]
public class TaxRatesController : ControllerBase
{
    private readonly IMediator _mediator;

    public TaxRatesController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    [Authorize(Roles = "Admin,Organization Manager,Outlet Manager,Vendor Manager")]
    public async Task<IActionResult> GetAll()
    {
        var response = await _mediator.Send(new GetAllTaxRatesQuery());
        return Ok(response);
    }

    [HttpGet("{taxRateID:int}")]
    [Authorize(Roles = "Admin,Organization Manager,Outlet Manager,Vendor Manager")]
    public async Task<IActionResult> GetById(int taxRateID)
    {
        var response = await _mediator.Send(new GetTaxRateByIdQuery(taxRateID));

        if (response.TaxRate == null)
            return NotFound();

        return Ok(response);
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Create(CreateTaxRateCommand command)
    {
        try
        {
            var response = await _mediator.Send(command);

            return CreatedAtAction(
                nameof(GetById),
                new { taxRateID = response.TaxRate.TaxRateID },
                response);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPut("{taxRateID:int}")]
    [ApiExplorerSettings(IgnoreApi = true)]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Update(
        int taxRateID,
        UpdateTaxRateCommand command)
    {
        try
        {
            command.TaxRateID = taxRateID;

            var response = await _mediator.Send(command);

            if (response.TaxRate == null)
                return NotFound();

            return Ok(response);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPost("{taxRateID:int}/activate")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Activate(int taxRateID)
    {
        var rate = await _mediator.Send(new GetTaxRateByIdQuery(taxRateID));
        if (rate.TaxRate == null) return NotFound();

        var updateCmd = new UpdateTaxRateCommand
        {
            TaxRateID = taxRateID,
            TaxName = rate.TaxRate.TaxName,
            Percentage = rate.TaxRate.Percentage,
            Status = "Active"
        };
        var res = await _mediator.Send(updateCmd);
        return Ok(res);
    }

    [HttpPost("{taxRateID:int}/deactivate")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Deactivate(int taxRateID)
    {
        var rate = await _mediator.Send(new GetTaxRateByIdQuery(taxRateID));
        if (rate.TaxRate == null) return NotFound();

        var updateCmd = new UpdateTaxRateCommand
        {
            TaxRateID = taxRateID,
            TaxName = rate.TaxRate.TaxName,
            Percentage = rate.TaxRate.Percentage,
            Status = "Inactive"
        };
        var res = await _mediator.Send(updateCmd);
        return Ok(res);
    }

    [HttpDelete("{taxRateID:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int taxRateID)
    {
        var response = await _mediator.Send(new DeleteTaxRateCommand(taxRateID));

        if (!response.Success)
            return NotFound();

        return NoContent();
    }
}