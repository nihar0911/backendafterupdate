namespace VendorManagementprojDomain.Entities;

public class Quotation
{
    public int QuotationID { get; set; }

    public int RequestID { get; set; }

    public int VendorID { get; set; }

    public DateTime ValidUntil { get; set; }

    public string Status { get; set; } = null!;

    public PurchaseRequest Request { get; set; } = null!;

    public ICollection<QuotationItem> QuotationItems { get; set; }
        = new List<QuotationItem>();
}