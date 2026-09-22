namespace VendorManagementprojDomain.Entities;

public class InvoiceItem
{
    public int InvoiceItemID { get; set; }
    public int InvoiceID { get; set; }
    public int ProductID { get; set; }
    public decimal Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal TaxRate { get; set; }
    public decimal Subtotal { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal TotalAmount { get; set; }

    public Invoice? Invoice { get; set; }
    public Product? Product { get; set; }
}