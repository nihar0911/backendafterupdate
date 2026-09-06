namespace VendorManagementprojApplication.DTOs;

public class UpdateTaxRateDto
{
    public string TaxName { get; set; } = string.Empty;

    public decimal Percentage { get; set; }

    public string Status { get; set; } = string.Empty;
}