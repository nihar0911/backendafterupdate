using VendorManagementprojApplication.DTOs;

namespace VendorManagementprojApplication.Features.Users.Commands.UpdateUser;

public class UpdateUserResponse
{
    public UserDto User { get; set; } = null!;
}
