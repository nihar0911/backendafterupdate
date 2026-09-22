using VendorManagementproj.Application.DTOs;

namespace VendorManagementproj.Application.Features.Users.Commands.CreateUser;

public class CreateUserResponse
{
    public UserDto User { get; set; } = null!;
}
