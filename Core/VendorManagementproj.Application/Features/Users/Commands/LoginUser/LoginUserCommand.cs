using MediatR;
using VendorManagementprojApplication.DTOs;

namespace VendorManagementprojApplication.Features.Users.Commands.LoginUser;

public class LoginUserCommand : IRequest<LoginUserResponse>
{
    public string Email { get; set; }
    public string Password { get; set; }

    public LoginUserCommand(string email, string password)
    {
        Email = email;
        Password = password;
    }
}