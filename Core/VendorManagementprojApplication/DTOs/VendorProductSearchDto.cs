namespace VendorManagementprojApplication.DTOs;

public class VendorProductSearchDto
{
    public int VendorProductID { get; set; }

    public int VendorID { get; set; }

    public int ProductID { get; set; }

    public decimal UnitPrice { get; set; }

    public int EstimatedDeliveryDays { get; set; }

    public string Status { get; set; } = string.Empty;

    public bool HasActiveContract { get; set; }

    public decimal AllocationPercentage { get; set; }

    public decimal AllocatedQuantity { get; set; }

    public decimal UsedQuantity { get; set; }

    public decimal RemainingQuantity { get; set; }
}