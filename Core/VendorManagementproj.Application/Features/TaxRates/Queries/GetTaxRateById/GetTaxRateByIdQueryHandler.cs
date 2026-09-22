using MediatR;
using VendorManagementproj.Application.Contracts.Persistence;
using VendorManagementproj.Application.DTOs;

namespace VendorManagementproj.Application.Features.TaxRates.Queries.GetTaxRateById;

public class GetTaxRateByIdQueryHandler : IRequestHandler<GetTaxRateByIdQuery, GetTaxRateByIdResponse>
{
    private readonly ITaxRateRepository _repository;

    public GetTaxRateByIdQueryHandler(ITaxRateRepository repository)
    {
        _repository = repository;
    }

    public async Task<GetTaxRateByIdResponse> Handle(GetTaxRateByIdQuery request, CancellationToken cancellationToken)
    {
        var taxRate = await _repository.GetByIdAsync(request.TaxRateID);

        if (taxRate == null)
            return new GetTaxRateByIdResponse { TaxRate = null };

        return new GetTaxRateByIdResponse
        {
            TaxRate = new TaxRateDto
            {
                TaxRateID = taxRate.TaxRateID,
                TaxName = taxRate.TaxName,
                Percentage = taxRate.Percentage,
                Status = taxRate.Status
            }
        };
    }
}
