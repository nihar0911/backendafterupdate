namespace VendorManagementprojDomain.Entities;

public class PurchaseOrder
{
    public int PurchaseOrderID { get; set; }
    public int RequestID { get; set; }
    public int VendorID { get; set; }
    public int QuotationID { get; set; }
    public int OutletID { get; set; }
    public DateTime OrderDate { get; set; }
    public DateTime? ExpectedDeliveryDate { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime? ActualDeliveryDate { get; set; }
    public DateTime? DispatchDateTime { get; set; }
    public string? DeliveryStatus { get; set; }

    public virtual PurchaseRequest? Request { get; set; }
    public virtual Vendor? Vendor { get; set; }
    public virtual Quotation? Quotation { get; set; }
    public virtual Outlet? Outlet { get; set; }

    public virtual ICollection<PurchaseOrderItem> Items { get; set; } = new List<PurchaseOrderItem>();
}