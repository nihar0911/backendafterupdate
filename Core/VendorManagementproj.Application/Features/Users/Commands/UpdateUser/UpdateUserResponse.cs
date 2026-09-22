using VendorManagementproj.Application.DTOs;

namespace VendorManagementproj.Application.Features.Users.Commands.UpdateUser;

public class UpdateUserResponse
{
    public UserDto User { get; set; } = null!;
}
