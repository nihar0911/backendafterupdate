using VendorManagementproj.Application.DTOs;

namespace VendorManagementproj.Application.Features.Users.Commands.UpdateMyProfile;

public class UpdateMyProfileResponse
{
    public LoginResponseDto? Profile { get; set; }
}
