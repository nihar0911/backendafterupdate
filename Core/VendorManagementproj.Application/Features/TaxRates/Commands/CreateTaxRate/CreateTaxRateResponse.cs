using VendorManagementproj.Application.DTOs;

namespace VendorManagementproj.Application.Features.TaxRates.Commands.CreateTaxRate;

public class CreateTaxRateResponse
{
    public TaxRateDto TaxRate { get; set; } = null!;
}
