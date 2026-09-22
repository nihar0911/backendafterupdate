namespace VendorManagementprojApplication.DTOs;

public class PurchaseRequestItemDto
{
    public int RequestItemID { get; set; }

    public int RequestID { get; set; }

    public int ProductID { get; set; }

    public string ProductName { get; set; } = string.Empty;

    public decimal Quantity { get; set; }

    public string Unit { get; set; } = string.Empty;
}
