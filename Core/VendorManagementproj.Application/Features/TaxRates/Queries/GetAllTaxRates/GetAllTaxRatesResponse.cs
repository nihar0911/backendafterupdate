using VendorManagementproj.Application.DTOs;

namespace VendorManagementproj.Application.Features.TaxRates.Queries.GetAllTaxRates;

public class GetAllTaxRatesResponse
{
    public List<TaxRateDto> TaxRates { get; set; } = new();
}
