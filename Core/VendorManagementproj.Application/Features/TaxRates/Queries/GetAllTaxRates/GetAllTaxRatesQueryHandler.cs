using MediatR;
using VendorManagementprojApplication.Contracts.Persistence;
using VendorManagementprojApplication.DTOs;

namespace VendorManagementprojApplication.Features.TaxRates.Queries.GetAllTaxRates;

public class GetAllTaxRatesQueryHandler : IRequestHandler<GetAllTaxRatesQuery, GetAllTaxRatesResponse>
{
    private readonly ITaxRateRepository _repository;

    public GetAllTaxRatesQueryHandler(ITaxRateRepository repository)
    {
        _repository = repository;
    }

    public async Task<GetAllTaxRatesResponse> Handle(GetAllTaxRatesQuery request, CancellationToken cancellationToken)
    {
        var taxRates = await _repository.GetAllAsync();

        var list = taxRates.Select(taxRate => new TaxRateDto
        {
            TaxRateID = taxRate.TaxRateID,
            TaxName = taxRate.TaxName,
            Percentage = taxRate.Percentage,
            Status = taxRate.Status
        }).ToList();

        return new GetAllTaxRatesResponse
        {
            TaxRates = list
        };
    }
}
