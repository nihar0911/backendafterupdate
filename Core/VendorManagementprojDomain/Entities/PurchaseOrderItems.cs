namespace VendorManagementprojDomain.Entities;

public class PurchaseOrderItem
{
    public int POItemID { get; set; }
    public int PurchaseOrderID { get; set; }
    public int ProductID { get; set; }
    public decimal Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal TaxRate { get; set; }
    public decimal Subtotal { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal TotalAmount { get; set; }

    public virtual PurchaseOrder? PurchaseOrder { get; set; }
    public virtual Product? Product { get; set; }
}