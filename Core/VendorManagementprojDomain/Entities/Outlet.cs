namespace VendorManagementprojDomain.Entities;

public class Outlet
{
  
    public int OutletID { get; set; }

    public int OrganizationID { get; set; }


    public string OutletName { get; set; } = string.Empty;

    public string? Address { get; set; }

    public decimal? Latitude { get; set; }

    public decimal? Longitude { get; set; }

    public Organization Organization { get; set; } = null!;
}