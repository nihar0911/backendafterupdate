using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VendorManagementprojApplication.Features.TaxRates.Commands.CreateTaxRate;
using VendorManagementprojApplication.Features.TaxRates.Commands.DeleteTaxRate;
using VendorManagementprojApplication.Features.TaxRates.Commands.UpdateTaxRate;
using VendorManagementprojApplication.Features.TaxRates.Queries.GetAllTaxRates;
using VendorManagementprojApplication.Features.TaxRates.Queries.GetTaxRateById;

namespace VendorManagementprojApi.Controllers;

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