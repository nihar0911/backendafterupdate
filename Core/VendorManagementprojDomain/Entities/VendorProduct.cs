namespace VendorManagementprojDomain.Entities;

public class VendorProduct
{
    public int VendorProductID { get; set; }

    public int VendorID { get; set; }

    public int ProductID { get; set; }

    public decimal UnitPrice { get; set; }

    public int EstimatedDeliveryDays { get; set; }

    public string Status { get; set; } = null!;

    public Product Product { get; set; } = null!;
}
