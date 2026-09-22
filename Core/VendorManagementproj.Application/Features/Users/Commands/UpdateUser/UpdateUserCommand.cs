using MediatR;
using VendorManagementprojApplication.DTOs;

namespace VendorManagementprojApplication.Features.Users.Commands.UpdateUser;

public class UpdateUserCommand : IRequest<UpdateUserResponse>
{
    public int UserID { get; set; }

    public int? OrganizationID { get; set; }

    public int? OutletID { get; set; }

    public int? VendorID { get; set; }

    public string Name { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string? Password { get; set; }

    public string Role { get; set; } = string.Empty;

    public int? RoleID { get; set; }
}
