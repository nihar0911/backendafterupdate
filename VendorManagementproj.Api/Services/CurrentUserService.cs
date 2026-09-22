using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using VendorManagementprojApplication.Contracts.Services;

namespace VendorManagementprojApi.Services;

public class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUserService(
        IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    private ClaimsPrincipal? User =>
        _httpContextAccessor.HttpContext?.User;

    public bool IsAuthenticated =>
        User?.Identity?.IsAuthenticated == true;

    public int? UserID =>
        GetIntClaim("UserID");

    public string? Name =>
        User?.FindFirst(ClaimTypes.Name)?.Value;

    public string? Email =>
        User?.FindFirst(ClaimTypes.Email)?.Value;

    public string? Role =>
        User?.FindFirst(ClaimTypes.Role)?.Value;

    public int? OrganizationID =>
        GetIntClaim("OrganizationID");

    public int? OutletID =>
        GetIntClaim("OutletID");

    public int? VendorID =>
        GetIntClaim("VendorID");

    public bool IsAdmin =>
        string.Equals(
            Role,
            "Admin",
            StringComparison.OrdinalIgnoreCase);

    public bool IsOrganizationManager =>
        string.Equals(
            Role,
            "Organization Manager",
            StringComparison.OrdinalIgnoreCase);

    public bool IsOutletManager =>
        string.Equals(
            Role,
            "Outlet Manager",
            StringComparison.OrdinalIgnoreCase);

    public bool IsVendorManager =>
        string.Equals(
            Role,
            "Vendor Manager",
            StringComparison.OrdinalIgnoreCase);

    public bool IsPurchaseManager =>
        string.Equals(
            Role,
            "Purchase Manager",
            StringComparison.OrdinalIgnoreCase);

    private int? GetIntClaim(string claimType)
    {
        string? value =
            User?.FindFirst(claimType)?.Value;

        if (int.TryParse(value, out int result))
            return result;

        return null;
    }
}