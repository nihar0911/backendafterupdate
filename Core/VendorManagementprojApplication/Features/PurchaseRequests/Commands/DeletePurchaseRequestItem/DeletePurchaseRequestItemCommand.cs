using MediatR;
using VendorManagementprojApplication.Contracts.Persistence;

namespace VendorManagementprojApplication.Features.PurchaseRequests.Commands.DeletePurchaseRequestItem;

public record DeletePurchaseRequestItemCommand(int RequestItemID) : IRequest<DeletePurchaseRequestItemResponse>;

public class DeletePurchaseRequestItemCommandHandler : IRequestHandler<DeletePurchaseRequestItemCommand, DeletePurchaseRequestItemResponse>
{
    private readonly IPurchaseRequestRepository _repository;

    public DeletePurchaseRequestItemCommandHandler(IPurchaseRequestRepository repository)
    {
        _repository = repository;
    }

    public async Task<DeletePurchaseRequestItemResponse> Handle(DeletePurchaseRequestItemCommand request, CancellationToken cancellationToken)
    {
        var success = await _repository.DeleteItemAsync(request.RequestItemID);
        return new DeletePurchaseRequestItemResponse
        {
            Success = success
        };
    }
}
