using MediatR;
using VendorManagementprojApplication.Contracts.Persistence;

namespace VendorManagementprojApplication.Features.TaxRates.Commands.DeleteTaxRate;

public class DeleteTaxRateCommandHandler : IRequestHandler<DeleteTaxRateCommand, DeleteTaxRateResponse>
{
    private readonly ITaxRateRepository _repository;

    public DeleteTaxRateCommandHandler(ITaxRateRepository repository)
    {
        _repository = repository;
    }

    public async Task<DeleteTaxRateResponse> Handle(DeleteTaxRateCommand request, CancellationToken cancellationToken)
    {
        var success = await _repository.DeleteAsync(request.TaxRateID);
        return new DeleteTaxRateResponse
        {
            Success = success
        };
    }
}
