namespace VendorManagementprojDomain.Entities;

public class Outlet
{
  
    public int OutletID { get; set; }

    public int OrganizationID { get; set; }


    public string OutletName { get; set; } = string.Empty;

    public string? Address { get; set; }

    public decimal? Latitude { get; set; }

    public decimal? Longitude { get; set; }

    /// <summary>
    /// Who must approve purchase orders for this outlet before they are placed with a vendor.
    /// "Organization Manager" or "Outlet Manager".
    /// </summary>
    public string PurchaseOrderApproverRole { get; set; } = "Organization Manager";

    public Organization Organization { get; set; } = null!;
}