namespace VendorManagementprojDomain.Entities;

public class QuotationItem
{
    public int QuotationItemID { get; set; }

    public int QuotationID { get; set; }

    public int ProductID { get; set; }

    public decimal Quantity { get; set; }

    public decimal UnitPrice { get; set; }

    public decimal DiscountAmount { get; set; } = 0;

    public decimal TaxRate { get; set; }

    public decimal TaxAmount { get; set; }

    public decimal TotalAmount { get; set; }

    public Quotation Quotation { get; set; } = null!;
    public virtual Product? Product { get; set; }
}