using MediatR;
using Microsoft.AspNetCore.Authorization;//helps to define roles nd all
using Microsoft.AspNetCore.Mvc;//gives api functionality hhtppost/httpget
using VendorManagementprojApplication.Features.Organizations.Commands.CreateOrganization;
using VendorManagementprojApplication.Features.Organizations.Commands.DeleteOrganization;
using VendorManagementprojApplication.Features.Organizations.Commands.UpdateOrganization;
using VendorManagementprojApplication.Features.Organizations.Queries.GetAllOrganizations;
using VendorManagementprojApplication.Features.Organizations.Queries.GetOrganizationById;

namespace VendorManagementprojApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class OrganizationsController : ControllerBase //controllerbase provides ok,notfound
{
    private readonly IMediator _mediator;

    public OrganizationsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    // Admin and Organization Manager can view organizations
      [HttpGet]
      [Authorize(Roles = "Admin,Organization Manager,Outlet Manager")]
      public async Task<IActionResult> GetAll()
      {
          var response =
              await _mediator.Send(
                  new GetAllOrganizationsQuery());

          return Ok(response);
      }

    // Admin and Organization Manager can view an organization
    [HttpGet("{id:int}")]
    [Authorize(Roles = "Admin,Organization Manager,Outlet Manager")]
    public async Task<IActionResult> GetById(int id)
    {
        var response =
            await _mediator.Send(
                new GetOrganizationByIdQuery(id));

        if (response.Organization == null)
            return NotFound();

        return Ok(response);
    }

    // Only Admin can create organizations
    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Create(
        CreateOrganizationCommand command)
    {

        try
        {
            var response =
                await _mediator.Send(command);

            return CreatedAtAction(
                nameof(GetById),
                new { id = response.Organization.OrganizationID },
                response);
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(
                new { message = ex.Message });
        }
    }

    // Admin and Organization Manager can update
    [HttpPut("{id:int}")]
   
    [Authorize(Roles = "Admin,Organization Manager")]
     public async Task<IActionResult> Update(
         int id,
         UpdateOrganizationCommand command)
     {
         try
         {
             command.OrganizationID = id;

             var response =
                 await _mediator.Send(command);

             if (response.Organization == null)
                 return NotFound();

             return Ok(response);
         }
         catch (InvalidOperationException ex)
         {
             return Conflict(
                 new { message = ex.Message });
         }
     }

     // Only Admin can delete organizations
     [HttpDelete("{id:int}")]
    [Authorize(Roles = "Admin")]
     public async Task<IActionResult> Delete(int id)
     {
         var response =
             await _mediator.Send(
                 new DeleteOrganizationCommand(id));

         if (!response.Success)
             return NotFound();

         return NoContent();
     }
 }
