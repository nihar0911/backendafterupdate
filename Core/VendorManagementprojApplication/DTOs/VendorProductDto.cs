namespace VendorManagementprojApplication.DTOs;

public class VendorProductDto
{
    public int VendorProductID { get; set; }
    public int VendorID { get; set; }
    public int ProductID { get; set; }
    public decimal UnitPrice { get; set; }
    public int EstimatedDeliveryDays { get; set; }
    public string Status { get; set; } = string.Empty;
}