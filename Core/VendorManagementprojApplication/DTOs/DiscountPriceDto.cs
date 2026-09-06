namespace VendorManagementprojApplication.DTOs;

public class DiscountPriceDto
{
    public int VendorID { get; set; }
    public int ProductID { get; set; }

    public decimal OriginalUnitPrice { get; set; }

    public string DiscountType { get; set; } = string.Empty;

    public decimal DiscountValue { get; set; }

    public decimal MinimumQuantity { get; set; }

    public decimal RequestedQuantity { get; set; }

    public decimal DiscountAmount { get; set; }

    public decimal EffectiveUnitPrice { get; set; }

    public bool DiscountApplied { get; set; }
}