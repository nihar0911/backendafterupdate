using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VendorManagementprojApplication.Features.Vendors.Commands.CreateVendor;
using VendorManagementprojApplication.Features.Vendors.Commands.DeleteVendor;
using VendorManagementprojApplication.Features.Vendors.Commands.UpdateVendor;
using VendorManagementprojApplication.Features.Vendors.Queries.GetAllVendors;
using VendorManagementprojApplication.Features.Vendors.Queries.GetVendorById;

namespace VendorManagementprojApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class VendorsController : ControllerBase
{
    private readonly IMediator _mediator;

    public VendorsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    [Authorize(Roles = "Admin,Organization Manager,Outlet Manager,Vendor Manager,Purchase Manager")]
    public async Task<IActionResult> GetAll()
    {
        var response =
            await _mediator.Send(
                new GetAllVendorsQuery());

        return Ok(response);
    }

    [HttpGet("{vendorID:int}")]
    [Authorize(Roles = "Admin,Organization Manager,Outlet Manager,Vendor Manager,Purchase Manager")]
    public async Task<IActionResult> GetById(
        int vendorID)
    {
        var response =
            await _mediator.Send(
                new GetVendorByIdQuery(vendorID));

        if (response.Vendor == null)
            return NotFound();

        return Ok(response);
    }

    [HttpPost]
    [Authorize(Roles = "Admin,Vendor Manager")]
    public async Task<IActionResult> Create(
        CreateVendorCommand command)
    {
        var response =
            await _mediator.Send(command);

        return CreatedAtAction(
            nameof(GetById),
            new { vendorID = response.Vendor.VendorID },
            response);
    }

    [HttpPut("{vendorID:int}")]

    [Authorize(Roles = "Admin,Vendor Manager")]

    public async Task<IActionResult> Update(
        int vendorID,
        UpdateVendorCommand command)
    {
        command.VendorID = vendorID;

        var response =
            await _mediator.Send(command);

        if (response.Vendor == null)
            return NotFound();

        return Ok(response);
    }

    [HttpDelete("{vendorID:int}")]
    [Authorize(Roles = "Admin,Vendor Manager")]

    public async Task<IActionResult> Delete(
        int vendorID)
    {
        var response =
            await _mediator.Send(
                new DeleteVendorCommand(vendorID));

        if (!response.Success)
            return NotFound();

        return NoContent();
    }
}