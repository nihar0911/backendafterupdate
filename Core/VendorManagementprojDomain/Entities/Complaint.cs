namespace VendorManagementprojDomain.Entities;

public class Complaint
{
    public int ComplaintID { get; set; }

    public int VendorID { get; set; }

    public int OutletID { get; set; }

    public int PurchaseOrderID { get; set; }

    public int POItemID { get; set; }

    public int ProductID { get; set; }

    public int RaisedByUserID { get; set; }

    public DateTime ComplaintDate { get; set; }

    public string ComplaintType { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public string Severity { get; set; } = "Medium";

    public string Status { get; set; } = "Active";

    public virtual Vendor? Vendor { get; set; }

    public virtual Outlet? Outlet { get; set; }

    public virtual PurchaseOrder? PurchaseOrder { get; set; }

    public virtual PurchaseOrderItem? POItem { get; set; }

    public virtual Product? Product { get; set; }

    public virtual User? RaisedByUser { get; set; }
}