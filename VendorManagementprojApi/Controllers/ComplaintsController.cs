using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VendorManagementprojApplication.Features.Complaints.Commands.CreateComplaint;
using VendorManagementprojApplication.Features.Complaints.Queries.GetAllComplaints;
using VendorManagementprojApplication.Features.Complaints.Queries.GetComplaintById;

namespace VendorManagementprojApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ComplaintsController : ControllerBase
{
    private readonly IMediator _mediator;

    public ComplaintsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost]
    [Authorize(Roles = "Outlet Manager")]
    public async Task<IActionResult> Create(
        [FromBody] CreateComplaintCommand command)
    {
        try
        {
            var response =
                await _mediator.Send(command);

            return Ok(response);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
    }

    [HttpGet]
    [Authorize(Roles = "Admin,Organization Manager,Outlet Manager")]
    public async Task<IActionResult> GetAll()
    {
        var response =
            await _mediator.Send(
                new GetAllComplaintsQuery());

        return Ok(response);
    }

    [HttpGet("{complaintID:int}")]
    [Authorize(Roles = "Admin,Organization Manager,Outlet Manager")]
    public async Task<IActionResult> GetById(
        int complaintID)
    {
        var response =
            await _mediator.Send(
                new GetComplaintByIdQuery
                {
                    ComplaintID = complaintID
                });

        if (response.Complaint == null)
            return NotFound();

        return Ok(response);
    }
}