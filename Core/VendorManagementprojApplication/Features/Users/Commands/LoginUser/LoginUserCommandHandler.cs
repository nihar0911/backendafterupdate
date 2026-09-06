using MediatR;
using VendorManagementprojApplication.Contracts.Persistence;
using VendorManagementprojApplication.Contracts.Services;
using VendorManagementprojApplication.DTOs;

namespace VendorManagementprojApplication.Features.Users.Commands.LoginUser;

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
            return new LoginUserResponse { Login = null };
        }

        var passwordValid =
            _passwordHasher.Verify(
                request.Password,
                user.PasswordHash);

        if (!passwordValid)
        {
            return new LoginUserResponse { Login = null };
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