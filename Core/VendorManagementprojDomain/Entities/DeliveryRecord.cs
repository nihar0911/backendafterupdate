namespace VendorManagementprojDomain.Entities;

public class DeliveryRecord
{
    public int DeliveryRecordID { get; set; }

    public int PurchaseOrderID { get; set; }

    public int POItemID { get; set; }

    public DateTime DeliveryDate { get; set; }

    public decimal OrderedQuantity { get; set; }

    public decimal ReceivedQuantity { get; set; }

    public decimal SpoiledQuantity { get; set; }

    public decimal SpoilagePercentage { get; set; }

    public string Status { get; set; } = "Pending";

    public int? ConfirmedByUserID { get; set; }

    public DateTime? ConfirmedAt { get; set; }

    public virtual PurchaseOrder? PurchaseOrder { get; set; }

    public virtual PurchaseOrderItem? PurchaseOrderItem { get; set; }

    public virtual User? ConfirmedByUser { get; set; }
}