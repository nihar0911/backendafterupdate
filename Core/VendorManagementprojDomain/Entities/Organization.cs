namespace VendorManagementprojDomain.Entities;

public class Organization
{
    public int OrganizationID { get; set; }

    public string OrganizationName { get; set; } = string.Empty;

    public string? Address { get; set; }

    public string? Phone { get; set; }

    public string? Email { get; set; }
    
}