using VendorManagementprojDomain.Entities;

namespace VendorManagementprojApplication.Contracts.Services;

public interface IJwtTokenService
{
    string GenerateToken(User user);
}