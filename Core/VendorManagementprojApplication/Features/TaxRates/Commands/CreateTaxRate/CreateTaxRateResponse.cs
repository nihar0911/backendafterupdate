using VendorManagementprojApplication.DTOs;

namespace VendorManagementprojApplication.Features.TaxRates.Commands.CreateTaxRate;

public class CreateTaxRateResponse
{
    public TaxRateDto TaxRate { get; set; } = null!;
}
