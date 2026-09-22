using MediatR;
using VendorManagementprojApplication.Contracts.Persistence;

namespace VendorManagementprojApplication.Features.PurchaseRequests.Commands.DeletePurchaseRequest;

public class DeletePurchaseRequestCommandHandler
    : IRequestHandler<DeletePurchaseRequestCommand, DeletePurchaseRequestResponse>
{
    private readonly IPurchaseRequestRepository _repository;

    public DeletePurchaseRequestCommandHandler(
        IPurchaseRequestRepository repository)
    {
        _repository = repository;
    }

    public async Task<DeletePurchaseRequestResponse> Handle(
        DeletePurchaseRequestCommand request,
        CancellationToken cancellationToken)
    {
        if (request.RequestID <= 0)
            throw new InvalidOperationException(
                "A valid purchase request is required.");

        var purchaseRequest =
            await _repository.GetByIdAsync(request.RequestID);

        if (purchaseRequest == null)
        {
            return new DeletePurchaseRequestResponse
            {
                Success = false
            };
        }

        if (!string.Equals(
            purchaseRequest.Status,
            "Pending",
            StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException(
                "Only a pending purchase request can be deleted.");
        }

        var success =
            await _repository.DeleteAsync(
                request.RequestID);

        return new DeletePurchaseRequestResponse
        {
            Success = success
        };
    }
}