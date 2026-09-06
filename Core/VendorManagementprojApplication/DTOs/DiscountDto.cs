namespace VendorManagementprojApplication.DTOs;

public class DiscountDto
{
    public int DiscountID { get; set; }
    public int VendorID { get; set; }
    public int ProductID { get; set; }
    public string DiscountName { get; set; } = string.Empty;
    public string DiscountType { get; set; } = string.Empty;
    public decimal DiscountValue { get; set; }
    public decimal MinimumQuantity { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public string Status { get; set; } = string.Empty;
}