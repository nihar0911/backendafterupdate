using MediatR;
using VendorManagementprojApplication.Contracts.Persistence;
using VendorManagementprojApplication.Contracts.Services;
using VendorManagementprojApplication.DTOs;
using VendorManagementprojDomain.Entities;

namespace VendorManagementprojApplication.Features.Users.Commands.CreateUser;

public class CreateUserCommandHandler
    : IRequestHandler<CreateUserCommand, CreateUserResponse>
{
    private readonly IUserRepository _userRepository;
    private readonly IOutletRepository _outletRepository;
    private readonly IPasswordHasher _passwordHasher;

    public CreateUserCommandHandler(
        IUserRepository userRepository,
        IOutletRepository outletRepository,
        IPasswordHasher passwordHasher)
    {
        _userRepository = userRepository;
        _outletRepository = outletRepository;
        _passwordHasher = passwordHasher;
    }

    public async Task<CreateUserResponse> Handle(
        CreateUserCommand request,
        CancellationToken cancellationToken)
    {
        if (await _userRepository.EmailExistsAsync(request.Email))
        {
            throw new InvalidOperationException(
                "Email already exists.");
        }

        int roleId;
        string resolvedRoleName = string.Empty;

        if (int.TryParse(request.Role, out var parsedRoleId))
        {
            roleId = parsedRoleId;
            var role = await _userRepository.GetRoleByIdAsync(roleId);
            if (role == null)
            {
                throw new InvalidOperationException($"Role ID '{parsedRoleId}' is invalid.");
            }
            resolvedRoleName = role.RoleName;
        }
        else
        {
            var roleName = request.Role?.Trim() ?? string.Empty;
            var role = await _userRepository.GetRoleByNameAsync(roleName);
            if (role != null)
            {
                roleId = role.RoleID;
                resolvedRoleName = role.RoleName;
            }
            else if (string.Equals(roleName, "OrganizationManager", StringComparison.OrdinalIgnoreCase))
            {
                var r = await _userRepository.GetRoleByNameAsync("Organization Manager");
                if (r != null) { roleId = r.RoleID; resolvedRoleName = r.RoleName; }
                else throw new InvalidOperationException($"Role '{request.Role}' is invalid.");
            }
            else if (string.Equals(roleName, "OutletManager", StringComparison.OrdinalIgnoreCase))
            {
                var r = await _userRepository.GetRoleByNameAsync("Outlet Manager");
                if (r != null) { roleId = r.RoleID; resolvedRoleName = r.RoleName; }
                else throw new InvalidOperationException($"Role '{request.Role}' is invalid.");
            }
            else if (string.Equals(roleName, "VendorManager", StringComparison.OrdinalIgnoreCase))
            {
                var r = await _userRepository.GetRoleByNameAsync("Vendor Manager");
                if (r != null) { roleId = r.RoleID; resolvedRoleName = r.RoleName; }
                else throw new InvalidOperationException($"Role '{request.Role}' is invalid.");
            }
            else
            {
                throw new InvalidOperationException($"Role '{request.Role}' is invalid.");
            }
        }

        int? finalOrganizationId = request.OrganizationID;
        int? finalOutletId = request.OutletID;

        if (string.Equals(resolvedRoleName, "Purchase Manager", StringComparison.OrdinalIgnoreCase))
        {
            // A. Mandatory Outlet
            if (!request.OutletID.HasValue || request.OutletID.Value <= 0)
            {
                throw new InvalidOperationException("Purchase Manager must be assigned to an Outlet.");
            }

            // B. Outlet Must Exist
            var outlet = await _outletRepository.GetByIdAsync(request.OutletID.Value);
            if (outlet == null)
            {
                throw new InvalidOperationException("The selected Outlet does not exist.");
            }

            // C. Organization and Outlet Must Match
            if (request.OrganizationID.HasValue && request.OrganizationID.Value > 0 && request.OrganizationID.Value != outlet.OrganizationID)
            {
                throw new InvalidOperationException("The selected Outlet does not belong to the specified Organization.");
            }
            finalOrganizationId = outlet.OrganizationID;
            finalOutletId = outlet.OutletID;

            // D. Only One Purchase Manager Per Outlet
            var existingPM = await _userRepository.GetPurchaseManagerByOutletIdAsync(outlet.OutletID);
            if (existingPM != null)
            {
                throw new InvalidOperationException("This Outlet already has a Purchase Manager assigned.");
            }
        }

        var user = new User
        {
            Name = request.Name,
            Email = request.Email,
            PasswordHash = _passwordHasher.Hash(request.Password),
            RoleID = roleId,
            OrganizationID = finalOrganizationId,
            OutletID = finalOutletId,
            VendorID = request.VendorID
        };

        var createdUser =
            await _userRepository.AddAsync(user);

        var dto = new UserDto
        {
            UserID = createdUser.UserID,
            Name = createdUser.Name,
            Email = createdUser.Email,
            RoleID = createdUser.RoleID,
            OrganizationID = createdUser.OrganizationID,
            OutletID = createdUser.OutletID,
            VendorID = createdUser.VendorID
        };

        return new CreateUserResponse
        {
            User = dto
        };
    }
}