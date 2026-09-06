using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using VendorManagementprojApplication.Contracts.Persistence;
using VendorManagementprojApplication.Contracts.Services;
using VendorManagementprojApplication.DTOs;

namespace VendorManagementprojApplication.Features.Users.Commands.UpdateMyProfile;

public class UpdateMyProfileCommandHandler
    : IRequestHandler<UpdateMyProfileCommand, UpdateMyProfileResponse>
{
    private readonly IUserRepository _userRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtTokenService _jwtTokenService;

    public UpdateMyProfileCommandHandler(
        IUserRepository userRepository,
        ICurrentUserService currentUserService,
        IPasswordHasher passwordHasher,
        IJwtTokenService jwtTokenService)
    {
        _userRepository = userRepository;
        _currentUserService = currentUserService;
        _passwordHasher = passwordHasher;
        _jwtTokenService = jwtTokenService;
    }

    public async Task<UpdateMyProfileResponse> Handle(
        UpdateMyProfileCommand request,
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

        var cleanEmail = request.Email.Trim();
        if (!string.Equals(user.Email, cleanEmail, StringComparison.OrdinalIgnoreCase))
        {
            var existingUser = await _userRepository.GetByEmailAsync(cleanEmail);
            if (existingUser != null && existingUser.UserID != user.UserID)
            {
                throw new InvalidOperationException("Email already exists.");
            }
        }

        user.Name = request.Name.Trim();
        user.Email = cleanEmail;

        if (!string.IsNullOrWhiteSpace(request.NewPassword))
        {
            user.PasswordHash = _passwordHasher.Hash(request.NewPassword);
        }

        await _userRepository.UpdateAsync(user);

        var updatedUser = await _userRepository.GetByIdAsync(user.UserID) ?? user;
        var token = _jwtTokenService.GenerateToken(updatedUser);

        var dto = new LoginResponseDto
        {
            Token = token,
            UserID = updatedUser.UserID,
            Name = updatedUser.Name,
            Email = updatedUser.Email,
            Role = updatedUser.Role?.RoleName ?? _currentUserService.Role ?? "Admin",
            OrganizationID = updatedUser.OrganizationID,
            OutletID = updatedUser.OutletID
        };

        return new UpdateMyProfileResponse
        {
            Profile = dto
        };
    }
}
