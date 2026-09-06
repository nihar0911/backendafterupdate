using MediatR;
using VendorManagementprojApplication.Contracts.Persistence;
using VendorManagementprojApplication.DTOs;
using VendorManagementprojDomain.Entities;

namespace VendorManagementprojApplication.Features.TaxRates.Commands.CreateTaxRate;

public class CreateTaxRateCommandHandler : IRequestHandler<CreateTaxRateCommand, CreateTaxRateResponse>
{
    private readonly ITaxRateRepository _repository;

    public CreateTaxRateCommandHandler(ITaxRateRepository repository)
    {
        _repository = repository;
    }

    public async Task<CreateTaxRateResponse> Handle(CreateTaxRateCommand request, CancellationToken cancellationToken)
    {
        if (request.Percentage < 0)
            throw new InvalidOperationException("Tax percentage cannot be negative.");

        var taxRate = new TaxRate
        {
            TaxName = request.TaxName,
            Percentage = request.Percentage,
            Status = request.Status
        };

        var created = await _repository.AddAsync(taxRate);

        var dto = new TaxRateDto
        {
            TaxRateID = created.TaxRateID,
            TaxName = created.TaxName,
            Percentage = created.Percentage,
            Status = created.Status
        };

        return new CreateTaxRateResponse
        {
            TaxRate = dto
        };
    }
}
