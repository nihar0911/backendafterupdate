namespace VendorManagementprojDomain.Entities;

public class User
{
    public int UserID { get; set; }

    public int? OrganizationID { get; set; }

    public int? OutletID { get; set; }

    public int? VendorID { get; set; }

    public string Name { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string PasswordHash { get; set; } = string.Empty;

    public int RoleID { get; set; }

    public Organization? Organization { get; set; }

    public Outlet? Outlet { get; set; }

    public Role? Role { get; set; }
}
