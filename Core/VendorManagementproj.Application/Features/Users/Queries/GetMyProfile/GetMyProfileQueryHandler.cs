using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using VendorManagementproj.Application.Contracts.Persistence;
using VendorManagementproj.Application.Contracts.Services;
using VendorManagementproj.Application.DTOs;

namespace VendorManagementproj.Application.Features.Users.Queries.GetMyProfile;

public class GetMyProfileQueryHandler
    : IRequestHandler<GetMyProfileQuery, GetMyProfileResponse>
{
    private readonly IUserRepository _userRepository;
    private readonly ICurrentUserService _currentUserService;

    public GetMyProfileQueryHandler(
        IUserRepository userRepository,
        ICurrentUserService currentUserService)
    {
        _userRepository = userRepository;
        _currentUserService = currentUserService;
    }

    public async Task<GetMyProfileResponse> Handle(
        GetMyProfileQuery request,
        CancellationToken cancellationToken)
    {
        var currentUserId = _currentUserService.UserID;
        if (!currentUserId.HasValue || currentUserId.Value <= 0)
        {
            throw new UnauthorizedAccessException("User is not authenticated.");
        }

        var user = await _userRepository.GetByIdAsync(currentUserId.Value);
        if (user == null)
        {
            throw new KeyNotFoundException("User not found.");
        }

        return new GetMyProfileResponse
        {
            User = new UserDto
            {
                UserID = user.UserID,
                Name = user.Name,
                Email = user.Email,
                RoleID = user.RoleID,
                RoleName = user.Role?.RoleName,
                OrganizationID = user.OrganizationID,
                OutletID = user.OutletID,
                VendorID = user.VendorID,
                Status = user.Status
            }
        };
    }
}
