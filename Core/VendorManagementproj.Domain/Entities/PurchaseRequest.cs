namespace VendorManagementprojDomain.Entities;

public class PurchaseRequest
{
    public int RequestID { get; set; }

    public int OutletID { get; set; }

    public int CreatedByUserID { get; set; }

    public DateTime RequestDate { get; set; }

    public string Status { get; set; } = null!;

    public virtual ICollection<PurchaseRequestItem> Items { get; set; } = new List<PurchaseRequestItem>();
}