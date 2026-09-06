using System.Collections.Generic;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VendorManagementprojApplication.DTOs;
using VendorManagementprojApplication.Features.Contracts.Commands.CreateContract;
using VendorManagementprojApplication.Features.Contracts.Commands.CreateContractFromQuotation;
using VendorManagementprojApplication.Features.Contracts.Commands.ResetContract;
using VendorManagementprojApplication.Features.Contracts.Commands.UpdateContract;
using VendorManagementprojApplication.Features.Contracts.Queries.GetAllContracts;
using VendorManagementprojApplication.Features.Contracts.Queries.GetContractById;
using VendorManagementprojApplication.Features.Contracts.Queries.GetContractsByOrganizationId;
using VendorManagementprojApplication.Features.Contracts.Queries.GetContractsByOutlet;

namespace VendorManagementprojApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ContractController : ControllerBase
{
    private readonly IMediator _mediator;

    public ContractController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    [Authorize(Roles = "Admin,Organization Manager,Outlet Manager,Vendor Manager")]
    public async Task<IActionResult> GetAll()
    {
        var result = await _mediator.Send(new GetAllContractsQuery());
        return Ok(result);
    }

    [HttpGet("{contractID:int}")]
    [Authorize(Roles = "Admin,Organization Manager,Outlet Manager,Vendor Manager")]
    public async Task<IActionResult> GetById(int contractID)
    {
        try
        {
            var result = await _mediator.Send(new GetContractByIdQuery(contractID));
            if (result.Contract == null)
                return NotFound();

            return Ok(result);
        }
        catch (UnauthorizedAccessException ex)
        {
            return StatusCode(403, new { message = ex.Message });
        }
    }

    [HttpGet("organization/{organizationID:int}")]
    [Authorize(Roles = "Admin,Organization Manager")]
    public async Task<IActionResult> GetByOrganization(int organizationID)
    {
        try
        {
            var result = await _mediator.Send(new GetContractsByOrganizationIdQuery(organizationID));
            return Ok(result);
        }
        catch (UnauthorizedAccessException ex)
        {
            return StatusCode(403, new { message = ex.Message });
        }
    }

    [HttpGet("outlet/{outletID:int}")]
    [Authorize(Roles = "Admin,Organization Manager,Outlet Manager")]
    public async Task<IActionResult> GetByOutlet(int outletID)
    {
        var result = await _mediator.Send(new GetContractsByOutletQuery(outletID));
        return Ok(result);
    }

    [HttpPost]
    [Authorize(Roles = "Admin,Organization Manager")]
    public async Task<IActionResult> Create([FromBody] CreateContractCommand command)
    {
        try
        {
            var result = await _mediator.Send(command);
            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (UnauthorizedAccessException ex)
        {
            return StatusCode(403, new { message = ex.Message });
        }
    }

    [HttpPost("from-quotation")]
    [Authorize(Roles = "Admin,Organization Manager")]
    public async Task<IActionResult> CreateFromQuotationWithBody([FromBody] CreateContractFromQuotationCommand command)
    {
        try
        {
            var result = await _mediator.Send(command);
            return Ok(result);
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
            return StatusCode(403, new { message = ex.Message });
        }
    }

    [HttpPost("from-quotation/{quotationID:int}")]
    [Authorize(Roles = "Admin,Organization Manager")]
    public async Task<IActionResult> CreateFromQuotation(int quotationID, [FromBody] List<CreateContractVendorAllocationDto>? allocations = null)
    {
        try
        {
            var result = await _mediator.Send(new CreateContractFromQuotationCommand 
            { 
                QuotationID = quotationID,
                Allocations = allocations 
            });
            return Ok(result);
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
            return StatusCode(403, new { message = ex.Message });
        }
    }

    [HttpPost("reset")]
    [Authorize(Roles = "Admin,Organization Manager")]
    public async Task<IActionResult> Reset([FromBody] ResetContractCommand command)
    {
        try
        {
            var result = await _mediator.Send(command);
            return Ok(result);
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

    [HttpPut]
    [Authorize(Roles = "Admin,Organization Manager")]
    public async Task<IActionResult> Update([FromBody] UpdateContractCommand command)
    {
        try
        {
            var result = await _mediator.Send(command);
            return Ok(result);
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
}
