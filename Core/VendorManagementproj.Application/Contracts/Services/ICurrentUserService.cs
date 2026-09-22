namespace VendorManagementprojApplication.Contracts.Services;

public interface ICurrentUserService
{
    int? UserID { get; }

    string? Name { get; }

    string? Email { get; }

    string? Role { get; }

    int? OrganizationID { get; }

    int? OutletID { get; }

    int? VendorID { get; }

    bool IsAuthenticated { get; }

    bool IsAdmin { get; }

    bool IsOrganizationManager { get; }

    bool IsOutletManager { get; }

    bool IsVendorManager { get; }

    bool IsPurchaseManager { get; }
}