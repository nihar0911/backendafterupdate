namespace VendorManagementprojDomain.Entities;

public class VendorFeedback
{
    public int FeedbackID { get; set; }

    public int VendorID { get; set; }

    public int OutletID { get; set; }

    public int PurchaseOrderID { get; set; }

    public int POItemID { get; set; }

    public int RatedByUserID { get; set; }

    public decimal Rating { get; set; }

    public decimal ProductQualityRating { get; set; }

    public decimal DeliveryRating { get; set; }

    public string Review { get; set; } = string.Empty;

    public DateTime FeedbackDate { get; set; }

    public virtual Vendor? Vendor { get; set; }

    public virtual Outlet? Outlet { get; set; }

    public virtual PurchaseOrder? PurchaseOrder { get; set; }

    public virtual PurchaseOrderItem? POItem { get; set; }

    public virtual User? RatedByUser { get; set; }
}