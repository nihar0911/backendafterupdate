namespace VendorManagementprojApplication.DTOs;

public class DeliveryRecordDto
{
    public int DeliveryRecordID { get; set; }

    public int PurchaseOrderID { get; set; }

    public int POItemID { get; set; }

    public DateTime DeliveryDate { get; set; }

    public decimal OrderedQuantity { get; set; }

    public decimal ReceivedQuantity { get; set; }

    public decimal SpoiledQuantity { get; set; }

    public decimal SpoilagePercentage { get; set; }

    public string Status { get; set; } = string.Empty;

    public int? ConfirmedByUserID { get; set; }

    public DateTime? ConfirmedAt { get; set; }
}