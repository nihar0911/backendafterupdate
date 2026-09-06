using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using VendorManagementprojApplication.Contracts.Persistence;
using VendorManagementprojApplication.Contracts.Services;
using VendorManagementprojApplication.DTOs;
using VendorManagementprojDomain.Entities;

namespace VendorManagementprojApplication.Features.Users.Commands.UpdateUser;

public class UpdateUserCommandHandler
    : IRequestHandler<UpdateUserCommand, UpdateUserResponse>
{
    private readonly IUserRepository _userRepository;
    private readonly IOutletRepository _outletRepository;
    private readonly IPasswordHasher _passwordHasher;

    public UpdateUserCommandHandler(
        IUserRepository userRepository,
        IOutletRepository outletRepository,
        IPasswordHasher passwordHasher)
    {
        _userRepository = userRepository;
        _outletRepository = outletRepository;
        _passwordHasher = passwordHasher;
    }

    public async Task<UpdateUserResponse> Handle(
        UpdateUserCommand request,
        CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByIdAsync(request.UserID);
        if (user == null)
        {
            throw new KeyNotFoundException($"User #{request.UserID} not found.");
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

        int roleId = request.RoleID.HasValue && request.RoleID.Value > 0 ? request.RoleID.Value : user.RoleID;
        string resolvedRoleName = user.Role?.RoleName ?? string.Empty;

        if (!string.IsNullOrWhiteSpace(request.Role))
        {
            var roleName = request.Role.Trim();
            if (int.TryParse(roleName, out var parsedRoleId))
            {
                roleId = parsedRoleId;
                var role = await _userRepository.GetRoleByIdAsync(roleId);
                if (role != null)
                {
                    resolvedRoleName = role.RoleName;
                }
            }
            else
            {
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
                }
                else if (string.Equals(roleName, "OutletManager", StringComparison.OrdinalIgnoreCase))
                {
                    var r = await _userRepository.GetRoleByNameAsync("Outlet Manager");
                    if (r != null) { roleId = r.RoleID; resolvedRoleName = r.RoleName; }
                }
                else if (string.Equals(roleName, "VendorManager", StringComparison.OrdinalIgnoreCase))
                {
                    var r = await _userRepository.GetRoleByNameAsync("Vendor Manager");
                    if (r != null) { roleId = r.RoleID; resolvedRoleName = r.RoleName; }
                }
            }
        }
        else if (request.RoleID.HasValue && request.RoleID.Value > 0)
        {
            var role = await _userRepository.GetRoleByIdAsync(request.RoleID.Value);
            if (role != null)
            {
                resolvedRoleName = role.RoleName;
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

            // D. Only One Purchase Manager Per Outlet (excluding current user)
            var existingPM = await _userRepository.GetPurchaseManagerByOutletIdAsync(outlet.OutletID);
            if (existingPM != null && existingPM.UserID != user.UserID)
            {
                throw new InvalidOperationException("This Outlet already has a Purchase Manager assigned.");
            }
        }

        user.Name = request.Name.Trim();
        user.Email = cleanEmail;
        user.RoleID = roleId;
        user.OrganizationID = finalOrganizationId;
        user.OutletID = finalOutletId;
        user.VendorID = request.VendorID;

        if (!string.IsNullOrWhiteSpace(request.Password))
        {
            user.PasswordHash = _passwordHasher.Hash(request.Password);
        }

        await _userRepository.UpdateAsync(user);

        var updatedUser = await _userRepository.GetByIdAsync(user.UserID) ?? user;

        var dto = new UserDto
        {
            UserID = updatedUser.UserID,
            Name = updatedUser.Name,
            Email = updatedUser.Email,
            RoleID = updatedUser.RoleID,
            RoleName = updatedUser.Role?.RoleName,
            OrganizationID = updatedUser.OrganizationID,
            OutletID = updatedUser.OutletID,
            VendorID = updatedUser.VendorID
        };

        return new UpdateUserResponse
        {
            User = dto
        };
    }
}
