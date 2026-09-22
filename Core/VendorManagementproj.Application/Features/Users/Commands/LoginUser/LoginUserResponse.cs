using VendorManagementproj.Application.DTOs;

namespace VendorManagementproj.Application.Features.Users.Commands.LoginUser;

public class LoginUserResponse
{
    public LoginResponseDto? Login { get; set; }
    public string? ErrorMessage { get; set; }
}
