using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using VendorManagementprojApplication.Contracts.Persistence;
using VendorManagementprojApplication.Contracts.Services;

namespace VendorManagementprojApplication.Features.PurchaseRequests.Commands.DeletePurchaseRequestItem;

public class DeletePurchaseRequestItemCommandHandler : IRequestHandler<DeletePurchaseRequestItemCommand, DeletePurchaseRequestItemResponse>
{
    private readonly IPurchaseRequestRepository _repository;
    private readonly ICurrentUserService _currentUserService;

    public DeletePurchaseRequestItemCommandHandler(
        IPurchaseRequestRepository repository,
        ICurrentUserService currentUserService)
    {
        _repository = repository;
        _currentUserService = currentUserService;
    }

    public async Task<DeletePurchaseRequestItemResponse> Handle(
        DeletePurchaseRequestItemCommand request,
        CancellationToken cancellationToken)
    {
        if (request.RequestItemID <= 0)
            throw new InvalidOperationException("A valid request item ID is required.");

        var allRequests = await _repository.GetAllAsync();
        var pr = allRequests.FirstOrDefault(r => r.Items != null && r.Items.Any(i => i.RequestItemID == request.RequestItemID));

        if (pr != null)
        {
            bool isAuthorized = false;
            if (_currentUserService.IsAdmin)
            {
                isAuthorized = true;
            }
            else if (_currentUserService.IsPurchaseManager && _currentUserService.OutletID.HasValue)
            {
                if (pr.OutletID == _currentUserService.OutletID.Value)
                {
                    isAuthorized = true;
                }
            }

            if (!isAuthorized)
            {
                throw new UnauthorizedAccessException("User is not authorized to delete items from this purchase request.");
            }
        }
        else if (!_currentUserService.IsAdmin && !_currentUserService.IsPurchaseManager)
        {
            throw new UnauthorizedAccessException("User is not authorized to delete purchase request items.");
        }

        var success = await _repository.DeleteItemAsync(request.RequestItemID);
        return new DeletePurchaseRequestItemResponse
        {
            Success = success
        };
    }
}
