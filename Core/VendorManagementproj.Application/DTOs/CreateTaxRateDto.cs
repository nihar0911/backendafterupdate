namespace VendorManagementprojApplication.DTOs;

public class CreateTaxRateDto
{
    public string TaxName { get; set; } = string.Empty;

    public decimal Percentage { get; set; }

    public string Status { get; set; } = string.Empty;
}