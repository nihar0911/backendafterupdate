using VendorManagementprojApplication.DTOs;

namespace VendorManagementprojApplication.Features.Users.Queries.GetAllUsers;

public class GetAllUsersResponse
{
    public List<UserDto> Users { get; set; } = new();
}
