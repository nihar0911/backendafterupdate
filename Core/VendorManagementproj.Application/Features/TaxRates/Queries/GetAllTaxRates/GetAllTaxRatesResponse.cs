using VendorManagementprojApplication.DTOs;

namespace VendorManagementprojApplication.Features.TaxRates.Queries.GetAllTaxRates;

public class GetAllTaxRatesResponse
{
    public List<TaxRateDto> TaxRates { get; set; } = new();
}
