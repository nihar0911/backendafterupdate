using VendorManagementprojApplication.DTOs;

namespace VendorManagementprojApplication.Features.Users.Commands.CreateUser;

public class CreateUserResponse
{
    public UserDto User { get; set; } = null!;
}
