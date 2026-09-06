namespace VendorManagementprojDomain.Entities;

public class PurchaseRequestItem
{
    public int RequestItemID { get; set; }

    public int RequestID { get; set; }

    public int ProductID { get; set; }

    public decimal Quantity { get; set; }

    public string Unit { get; set; } = null!;

    public virtual PurchaseRequest? Request { get; set; }

    public virtual Product? Product { get; set; }
}
