using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VendorManagementprojApplication.Features.Outlets.Commands.CreateOutlet;
using VendorManagementprojApplication.Features.Outlets.Commands.DeleteOutlet;
using VendorManagementprojApplication.Features.Outlets.Commands.UpdateOutlet;
using VendorManagementprojApplication.Features.Outlets.Queries.GetAllOutlets;
using VendorManagementprojApplication.Features.Outlets.Queries.GetOutletById;
using VendorManagementprojApplication.Features.Outlets.Queries.GetOutletsByOrganizationId;

namespace VendorManagementprojApi.Controllers;

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
    [Authorize(Roles = "Admin,Organization Manager,Outlet Manager")]
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
}