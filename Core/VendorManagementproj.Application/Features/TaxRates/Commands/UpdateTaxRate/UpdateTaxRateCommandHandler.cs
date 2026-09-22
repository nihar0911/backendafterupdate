using MediatR;
using VendorManagementprojApplication.Contracts.Persistence;
using VendorManagementprojApplication.DTOs;
using VendorManagementprojDomain.Entities;

namespace VendorManagementprojApplication.Features.TaxRates.Commands.UpdateTaxRate;

public class UpdateTaxRateCommandHandler : IRequestHandler<UpdateTaxRateCommand, UpdateTaxRateResponse>
{
    private readonly ITaxRateRepository _repository;

    public UpdateTaxRateCommandHandler(ITaxRateRepository repository)
    {
        _repository = repository;
    }

    public async Task<UpdateTaxRateResponse> Handle(UpdateTaxRateCommand request, CancellationToken cancellationToken)
    {
        if (request.Percentage < 0)
            throw new InvalidOperationException("Tax percentage cannot be negative.");

        var taxRate = new TaxRate
        {
            TaxName = request.TaxName,
            Percentage = request.Percentage,
            Status = request.Status
        };

        var updated = await _repository.UpdateAsync(request.TaxRateID, taxRate);

        if (updated == null)
            return new UpdateTaxRateResponse { TaxRate = null };

        return new UpdateTaxRateResponse
        {
            TaxRate = new TaxRateDto
            {
                TaxRateID = updated.TaxRateID,
                TaxName = updated.TaxName,
                Percentage = updated.Percentage,
                Status = updated.Status
            }
        };
    }
}
