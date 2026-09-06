namespace VendorManagementprojApplication.DTOs;

public class PurchaseOrderDto
{
    public int PurchaseOrderID { get; set; }
    public int RequestID { get; set; }
    public int VendorID { get; set; }
    public int QuotationID { get; set; }
    public int OutletID { get; set; }
    public DateTime OrderDate { get; set; }
    public DateTime? ExpectedDeliveryDate { get; set; }
    public DateTime? DispatchDateTime { get; set; }
    public DateTime? ActualDeliveryDate { get; set; }
    public string? DeliveryStatus { get; set; }
    public string Status { get; set; } = string.Empty;
    public List<PurchaseOrderItemDto> Items { get; set; } = new();
}