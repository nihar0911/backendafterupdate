namespace VendorManagementprojDomain.Entities;

public class TaxRate
{
    public int TaxRateID { get; set; }

    public string TaxName { get; set; } = string.Empty;

    public decimal Percentage { get; set; }

    public string Status { get; set; } = string.Empty;
}