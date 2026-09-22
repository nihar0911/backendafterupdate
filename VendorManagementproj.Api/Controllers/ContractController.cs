using System.Collections.Generic;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VendorManagementprojApplication.Contracts.Services;
using VendorManagementprojApplication.DTOs;
using VendorManagementprojApplication.Features.Contracts.Commands.CreateContract;
using VendorManagementprojApplication.Features.Contracts.Commands.CreateContractFromQuotation;
using VendorManagementprojApplication.Features.Contracts.Commands.EndContract;
using VendorManagementprojApplication.Features.Contracts.Commands.RenewContract;
using VendorManagementprojApplication.Features.Contracts.Commands.ResetContract;
using VendorManagementprojApplication.Features.Contracts.Commands.UpdateContract;
using VendorManagementprojApplication.Features.Contracts.Queries.GetAllContracts;
using VendorManagementprojApplication.Features.Contracts.Queries.GetContractById;
using VendorManagementprojApplication.Features.Contracts.Queries.GetContractsByOrganizationId;
using VendorManagementprojApplication.Features.Contracts.Queries.GetContractsByOutlet;
using VendorManagementprojApplication.Features.Contracts.Queries.GetActiveContractsForProduct;
using VendorManagementprojApplication.Features.Contracts.Queries.GetContractEligibleVendors;

namespace VendorManagementprojApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ContractController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ICurrentUserService _currentUserService;

    public ContractController(IMediator mediator, ICurrentUserService currentUserService)
    {
        _mediator = mediator;
        _currentUserService = currentUserService;
    }

    [HttpGet]
    [Authorize(Roles = "Admin,Organization Manager,Outlet Manager,Vendor Manager,Purchase Manager")]
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

    [HttpGet("outlet/my")]
    [HttpGet("my")]
    [Authorize(Roles = "Outlet Manager")]
    public async Task<IActionResult> GetMyContracts()
    {
        if (!_currentUserService.OutletID.HasValue)
            return Ok(new GetContractsByOutletResponse { Contracts = new List<ContractDto>() });

        var result = await _mediator.Send(new GetContractsByOutletQuery(_currentUserService.OutletID.Value));
        return Ok(result);
    }

    [HttpGet("outlet/{outletID:int}")]
    [Authorize(Roles = "Admin,Organization Manager,Outlet Manager")]
    public async Task<IActionResult> GetByOutlet(int outletID)
    {
        var result = await _mediator.Send(new GetContractsByOutletQuery(outletID));
        return Ok(result);
    }

    [HttpGet("active/{outletID:int}/{productID:int}")]
    [Authorize(Roles = "Admin,Organization Manager,Outlet Manager,Purchase Manager")]
    public async Task<IActionResult> GetActiveContractsForProduct(int outletID, int productID)
    {
        var result = await _mediator.Send(new GetActiveContractsForProductQuery(outletID, productID));
        return Ok(result);
    }

    [HttpGet("eligible-vendors/{outletID:int}/{productID:int}")]
    [Authorize(Roles = "Admin,Organization Manager,Outlet Manager")]
    public async Task<IActionResult> GetEligibleVendorsForContract(int outletID, int productID)
    {
        try
        {
            var result = await _mediator.Send(new GetContractEligibleVendorsQuery(outletID, productID));
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

    [HttpPost("{contractID:int}/end")]
    [Authorize(Roles = "Admin,Organization Manager")]
    public async Task<IActionResult> End(int contractID)
    {
        try
        {
            var result = await _mediator.Send(new EndContractCommand { ContractID = contractID });
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

    [HttpPost("end")]
    [Authorize(Roles = "Admin,Organization Manager")]
    public async Task<IActionResult> EndWithBody([FromBody] EndContractCommand command)
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

    [HttpPost("{contractID:int}/renew")]
    [Authorize(Roles = "Admin,Organization Manager")]
    public async Task<IActionResult> Renew(int contractID, [FromBody] RenewContractCommand command)
    {
        try
        {
            command.ContractID = contractID;
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
}
