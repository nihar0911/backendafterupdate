namespace VendorManagementprojDomain.Entities;

public class Invoice
{
    public int InvoiceID { get; set; }
    public int PurchaseOrderID { get; set; }
    public int VendorID { get; set; }
    public int OutletID { get; set; }
    public DateTime InvoiceDate { get; set; }
    public decimal Subtotal { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal TotalAmount { get; set; }
    public string Status { get; set; } = "Pending";
    public string? InvoiceDocumentBase64 { get; set; }
    public string? InvoiceFileName { get; set; }
    public string? InvoiceContentType { get; set; } 



    public PurchaseOrder? PurchaseOrder { get; set; }
    public Vendor? Vendor { get; set; }
    public Outlet? Outlet { get; set; }
    
    public ICollection<InvoiceItem> Items { get; set; } = new List<InvoiceItem>();
}