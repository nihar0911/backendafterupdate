using MediatR;
using VendorManagementproj.Application.Contracts.Persistence;
using VendorManagementproj.Application.Contracts.Services;
using VendorManagementproj.Application.DTOs;

namespace VendorManagementproj.Application.Features.Users.Commands.LoginUser;

public class LoginUserCommandHandler
    : IRequestHandler<LoginUserCommand, LoginUserResponse>
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtTokenService _jwtTokenService;

    public LoginUserCommandHandler(
        IUserRepository userRepository,
        IPasswordHasher passwordHasher,
        IJwtTokenService jwtTokenService)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
        _jwtTokenService = jwtTokenService;
    }

    public async Task<LoginUserResponse> Handle(
        LoginUserCommand request,
        CancellationToken cancellationToken)
    {
        var user =
            await _userRepository
                .GetByEmailAsync(request.Email);

        if (user == null)
        {
            return new LoginUserResponse { Login = null, ErrorMessage = "Invalid email or password." };
        }

        if (!string.Equals(user.Status, "Active", StringComparison.OrdinalIgnoreCase))
        {
            return new LoginUserResponse
            {
                Login = null,
                ErrorMessage = "Account is deactivated."
            };
        }

        var passwordValid =
            _passwordHasher.Verify(
                request.Password,
                user.PasswordHash);

        if (!passwordValid)
        {
            return new LoginUserResponse { Login = null, ErrorMessage = "Invalid email or password." };
        }

        var token =
            _jwtTokenService.GenerateToken(user);

        var dto = new LoginResponseDto
        {
            Token = token,
            UserID = user.UserID,
            Name = user.Name,
            Email = user.Email,
            Role =
                user.Role?.RoleName
                ?? string.Empty,
            OrganizationID =
                user.OrganizationID,
            OutletID =
                user.OutletID,
            VendorID =
                user.VendorID
        };

        return new LoginUserResponse
        {
            Login = dto
        };
    }
}