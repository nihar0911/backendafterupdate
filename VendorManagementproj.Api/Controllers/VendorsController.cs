using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using VendorManagementproj.Application.Contracts.Services;
using VendorManagementproj.Application.Features.Vendors.Commands.CreateVendor;
using VendorManagementproj.Application.Features.Vendors.Commands.DeleteVendor;
using VendorManagementproj.Application.Features.Vendors.Commands.UpdateVendor;
using VendorManagementproj.Application.Features.Vendors.Queries.GetAllVendors;
using VendorManagementproj.Application.Features.Vendors.Queries.GetVendorById;

namespace VendorManagementproj.Api.Controllers;

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
        UpdateVendorCommand command,
        [FromServices] ICurrentUserService currentUserService)
    {
        if (!currentUserService.IsAdmin && currentUserService.IsVendorManager)
        {
            if (!currentUserService.VendorID.HasValue || currentUserService.VendorID.Value != vendorID)
            {
                return StatusCode(StatusCodes.Status403Forbidden, new { message = "You are not authorized to update another vendor's profile." });
            }
        }

        command.VendorID = vendorID;

        var response =
            await _mediator.Send(command);

        if (response.Vendor == null)
            return NotFound();

        return Ok(response);
    }

    [HttpPost("{vendorID:int}/activate")]
    [Authorize(Roles = "Admin,Vendor Manager")]
    public async Task<IActionResult> Activate(
        int vendorID,
        [FromServices] ICurrentUserService currentUserService)
    {
        if (!currentUserService.IsAdmin && currentUserService.IsVendorManager)
        {
            if (!currentUserService.VendorID.HasValue || currentUserService.VendorID.Value != vendorID)
            {
                return StatusCode(StatusCodes.Status403Forbidden, new { message = "You are not authorized to modify another vendor's status." });
            }
        }

        var response = await _mediator.Send(new GetVendorByIdQuery(vendorID));
        if (response.Vendor == null) return NotFound();

        var updateCmd = new UpdateVendorCommand
        {
            VendorID = vendorID,
            VendorName = response.Vendor.VendorName,
            Email = response.Vendor.Email,
            Phone = response.Vendor.Phone,
            Address = response.Vendor.Address,
            GSTIN = response.Vendor.GSTIN,
            Status = "Active"
        };
        var res = await _mediator.Send(updateCmd);
        return Ok(res);
    }

    [HttpPost("{vendorID:int}/deactivate")]
    [Authorize(Roles = "Admin,Vendor Manager")]
    public async Task<IActionResult> Deactivate(
        int vendorID,
        [FromServices] ICurrentUserService currentUserService)
    {
        if (!currentUserService.IsAdmin && currentUserService.IsVendorManager)
        {
            if (!currentUserService.VendorID.HasValue || currentUserService.VendorID.Value != vendorID)
            {
                return StatusCode(StatusCodes.Status403Forbidden, new { message = "You are not authorized to modify another vendor's status." });
            }
        }

        var response = await _mediator.Send(new GetVendorByIdQuery(vendorID));
        if (response.Vendor == null) return NotFound();

        var updateCmd = new UpdateVendorCommand
        {
            VendorID = vendorID,
            VendorName = response.Vendor.VendorName,
            Email = response.Vendor.Email,
            Phone = response.Vendor.Phone,
            Address = response.Vendor.Address,
            GSTIN = response.Vendor.GSTIN,
            Status = "Inactive"
        };
        var res = await _mediator.Send(updateCmd);
        return Ok(res);
    }

    [HttpDelete("{vendorID:int}")]
    [Authorize(Roles = "Admin,Vendor Manager")]
    public async Task<IActionResult> Delete(
        int vendorID,
        [FromServices] ICurrentUserService currentUserService)
    {
        if (!currentUserService.IsAdmin && currentUserService.IsVendorManager)
        {
            if (!currentUserService.VendorID.HasValue || currentUserService.VendorID.Value != vendorID)
            {
                return StatusCode(StatusCodes.Status403Forbidden, new { message = "You are not authorized to delete another vendor." });
            }
        }

        var response =
            await _mediator.Send(
                new DeleteVendorCommand(vendorID));

        if (!response.Success)
            return NotFound();

        return NoContent();
    }
}