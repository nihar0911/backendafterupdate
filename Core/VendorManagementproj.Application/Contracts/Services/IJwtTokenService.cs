using VendorManagementproj.Domain.Entities;

namespace VendorManagementproj.Application.Contracts.Services;

public interface IJwtTokenService
{
    string GenerateToken(User user);
}