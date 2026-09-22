using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VendorManagementproj.Application.Features.Users.Commands.CreateUser;
using VendorManagementproj.Application.Features.Users.Commands.UpdateMyProfile;
using VendorManagementproj.Application.Features.Users.Commands.UpdateUser;
using VendorManagementproj.Application.Features.Users.Queries.GetAllUsers;
using VendorManagementproj.Application.Features.Users.Queries.GetMyProfile;
using VendorManagementproj.Application.Features.Users.Queries.GetUserById;

namespace VendorManagementproj.Api.Controllers;

[ApiController]
[Route("api/users")]
[Authorize]
public class UsersController : ControllerBase
{
    private readonly IMediator _mediator;

    public UsersController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetAll()
    {
        var response =
            await _mediator.Send(
                new GetAllUsersQuery());

        return Ok(response);
    }

    [HttpGet("me")]
    public async Task<IActionResult> GetMyProfile()
    {
        try
        {
            var response =
                await _mediator.Send(
                    new GetMyProfileQuery());

            if (response.User == null)
                return NotFound();

            return Ok(response);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(
                new { message = ex.Message });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(
                new { message = ex.Message });
        }
    }

    [HttpGet("{userID:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetById(
        int userID)
    {
        var response =
            await _mediator.Send(
                new GetUserByIdQuery(userID));

        if (response.User == null)
            return NotFound();

        return Ok(response);
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Create(
        CreateUserCommand command)
    {
        try
        {
            var response =
                await _mediator.Send(command);

            return Ok(response);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(
                new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(
                new { message = ex.Message });
        }
    }

    [HttpPut("{userID:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Update(
        int userID,
        UpdateUserCommand command)
    {
        try
        {
            command.UserID = userID;
            var response =
                await _mediator.Send(command);

            return Ok(response);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(
                new { message = ex.Message });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(
                new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(
                new { message = ex.Message });
        }
    }

    [HttpPut("me")]
    public async Task<IActionResult> UpdateMyProfile(
        UpdateMyProfileCommand command)
    {
        try
        {
            var response =
                await _mediator.Send(command);

            return Ok(response);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(
                new { message = ex.Message });
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(
                new { message = ex.Message });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(
                new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(
                new { message = ex.Message });
        }
    }

    [HttpPost("{userID:int}/activate")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Activate(int userID, [FromServices] VendorManagementproj.Application.Contracts.Persistence.IUserRepository repo)
    {
        var user = await repo.GetByIdAsync(userID);
        if (user == null) return NotFound();

        user.Status = "Active";
        await repo.UpdateAsync(user);

        return Ok(new VendorManagementproj.Application.DTOs.UserDto
        {
            UserID = user.UserID,
            Name = user.Name,
            Email = user.Email,
            RoleID = user.RoleID,
            RoleName = user.Role?.RoleName,
            OrganizationID = user.OrganizationID,
            OutletID = user.OutletID,
            VendorID = user.VendorID,
            Status = user.Status
        });
    }

    [HttpPost("{userID:int}/deactivate")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Deactivate(
        int userID,
        [FromServices] VendorManagementproj.Application.Contracts.Persistence.IUserRepository repo,
        [FromServices] VendorManagementproj.Application.Contracts.Services.ICurrentUserService currentUserService)
    {
        if (currentUserService.UserID.HasValue && currentUserService.UserID.Value == userID)
        {
            return BadRequest(new { message = "You cannot deactivate your own currently logged-in account." });
        }

        var user = await repo.GetByIdAsync(userID);
        if (user == null) return NotFound();

        user.Status = "Inactive";
        await repo.UpdateAsync(user);

        return Ok(new VendorManagementproj.Application.DTOs.UserDto
        {
            UserID = user.UserID,
            Name = user.Name,
            Email = user.Email,
            RoleID = user.RoleID,
            RoleName = user.Role?.RoleName,
            OrganizationID = user.OrganizationID,
            OutletID = user.OutletID,
            VendorID = user.VendorID,
            Status = user.Status
        });
    }
}
