using MediatR;

namespace VendorManagementproj.Application.Features.Users.Commands.UpdateMyProfile;

public class UpdateMyProfileCommand : IRequest<UpdateMyProfileResponse>
{
    public string Name { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string? NewPassword { get; set; }
}
