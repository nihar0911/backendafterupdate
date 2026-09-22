using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VendorManagementproj.Application.Features.Outlets.Commands.CreateOutlet;
using VendorManagementproj.Application.Features.Outlets.Commands.DeleteOutlet;
using VendorManagementproj.Application.Features.Outlets.Commands.UpdateOutlet;
using VendorManagementproj.Application.Features.Outlets.Queries.GetAllOutlets;
using VendorManagementproj.Application.Features.Outlets.Queries.GetOutletById;
using VendorManagementproj.Application.Features.Outlets.Queries.GetOutletsByOrganizationId;

namespace VendorManagementproj.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class OutletsController : ControllerBase
{
    private readonly IMediator _mediator;

    public OutletsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    [Authorize(Roles = "Admin,Organization Manager,Outlet Manager,Purchase Manager")]
    public async Task<IActionResult> GetAll()
    {
        var response = await _mediator.Send(new GetAllOutletsQuery());
        return Ok(response);
    }

    [HttpGet("{id:int}")]
    [Authorize(Roles = "Admin,Organization Manager,Outlet Manager")]
    public async Task<IActionResult> GetById(int id)
    {
        var response =
            await _mediator.Send(
                new GetOutletByIdQuery(id));

        if (response.Outlet == null)
            return NotFound();

        return Ok(response);
    }

    [HttpPost]
    [Authorize(Roles = "Admin,Organization Manager")]
    public async Task<IActionResult> Create(
        CreateOutletCommand command)
    {
        
        var response =
            await _mediator.Send(command);

        return CreatedAtAction(
            nameof(GetById),
            new { id = response.Outlet.OutletID },
            response);
    }

    [HttpGet("organization/{organizationId:int}")]
    
    [Authorize(Roles = "Admin,Organization Manager")]
    public async Task<IActionResult> GetByOrganizationId(
        int organizationId)
    {
        var response =
            await _mediator.Send(
                new GetOutletsByOrganizationIdQuery(
                    organizationId));
        return Ok(response);
    }

    [HttpPut("{id:int}")]
   


    [Authorize(Roles = "Admin,Organization Manager")]
    public async Task<IActionResult> Update(
        int id,
        UpdateOutletCommand command)
    {
        command.OutletID = id;

        var response =
            await _mediator.Send(command);

        if (response.Outlet == null)
            return NotFound();

        return Ok(response);
    }

    [HttpDelete("{id:int}")]
   
    [Authorize(Roles = "Admin,Organization Manager")]
    public async Task<IActionResult> Delete(int id)
    {
        var response =
            await _mediator.Send(
                new DeleteOutletCommand(id));

        if (!response.Success)
            return NotFound();

        return NoContent();
    }

    [HttpPost("{id:int}/activate")]
    [Authorize(Roles = "Admin,Organization Manager")]
    public async Task<IActionResult> Activate(int id, [FromServices] VendorManagementproj.Application.Contracts.Persistence.IOutletRepository repo)
    {
        var outlet = await repo.GetByIdAsync(id);
        if (outlet == null) return NotFound();

        outlet.Status = "Active";
        var updated = await repo.UpdateAsync(outlet);

        return Ok(new VendorManagementproj.Application.DTOs.OutletDto
        {
            OutletID = updated.OutletID,
            OrganizationID = updated.OrganizationID,
            OutletName = updated.OutletName,
            Address = updated.Address,
            Latitude = updated.Latitude,
            Longitude = updated.Longitude,
            PurchaseOrderApproverRole = updated.PurchaseOrderApproverRole,
            Status = updated.Status
        });
    }

    [HttpPost("{id:int}/deactivate")]
    [Authorize(Roles = "Admin,Organization Manager")]
    public async Task<IActionResult> Deactivate(int id, [FromServices] VendorManagementproj.Application.Contracts.Persistence.IOutletRepository repo)
    {
        var outlet = await repo.GetByIdAsync(id);
        if (outlet == null) return NotFound();

        outlet.Status = "Inactive";
        var updated = await repo.UpdateAsync(outlet);

        return Ok(new VendorManagementproj.Application.DTOs.OutletDto
        {
            OutletID = updated.OutletID,
            OrganizationID = updated.OrganizationID,
            OutletName = updated.OutletName,
            Address = updated.Address,
            Latitude = updated.Latitude,
            Longitude = updated.Longitude,
            PurchaseOrderApproverRole = updated.PurchaseOrderApproverRole,
            Status = updated.Status
        });
    }
}