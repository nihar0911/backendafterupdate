using MediatR;
using VendorManagementprojApplication.Contracts.Persistence;

namespace VendorManagementprojApplication.Features.Quotations.Commands.DeleteQuotation;

public class DeleteQuotationCommandHandler : IRequestHandler<DeleteQuotationCommand, DeleteQuotationResponse>
{
    private readonly IQuotationRepository _quotationRepository;

    public DeleteQuotationCommandHandler(IQuotationRepository quotationRepository)
    {
        _quotationRepository = quotationRepository;
    }

    public async Task<DeleteQuotationResponse> Handle(DeleteQuotationCommand request, CancellationToken cancellationToken)
    {
        var success = await _quotationRepository.DeleteAsync(request.QuotationID);
        return new DeleteQuotationResponse
        {
            Success = success
        };
    }
}
