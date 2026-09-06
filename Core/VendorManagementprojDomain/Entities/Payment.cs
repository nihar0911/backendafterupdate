namespace VendorManagementprojDomain.Entities;

public class Payment
{
    public int PaymentID { get; set; }

    public int InvoiceID { get; set; }

    public DateTime PaymentDate { get; set; }

    public decimal Amount { get; set; }

    public string PaymentMethod { get; set; } = string.Empty;

    public string? TransactionReference { get; set; }

    public string Status { get; set; } = "Paid";

    public virtual Invoice? Invoice { get; set; }
}