using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VendorManagementprojApplication.Features.Users.Commands.CreateUser;
using VendorManagementprojApplication.Features.Users.Commands.UpdateMyProfile;
using VendorManagementprojApplication.Features.Users.Commands.UpdateUser;
using VendorManagementprojApplication.Features.Users.Queries.GetAllUsers;
using VendorManagementprojApplication.Features.Users.Queries.GetUserById;

namespace VendorManagementprojApi.Controllers;

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
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> UpdateMyProfile(
        UpdateMyProfileCommand command)
    {
        try
        {
            var response =
                await _mediator.Send(command);

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
        catch (InvalidOperationException ex)
        {
            return Conflict(
                new { message = ex.Message });
        }
    }
}
