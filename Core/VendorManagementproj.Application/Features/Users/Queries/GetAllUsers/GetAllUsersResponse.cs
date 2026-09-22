using VendorManagementproj.Application.DTOs;

namespace VendorManagementproj.Application.Features.Users.Queries.GetAllUsers;

public class GetAllUsersResponse
{
    public List<UserDto> Users { get; set; } = new();
}
