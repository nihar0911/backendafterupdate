using MediatR;
using Microsoft.AspNetCore.Mvc;
using VendorManagementprojApplication.Features.Users.Commands.LoginUser;

namespace VendorManagementprojApi.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly IMediator _mediator;

    public AuthController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody]
        LoginUserCommand command)
    {
        var response = await _mediator.Send(command);

        if (response.Login == null)
            return Unauthorized("Invalid email or password.");

        return Ok(response);
    }
}