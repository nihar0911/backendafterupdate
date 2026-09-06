using MediatR;
using VendorManagementprojApplication.Contracts.Persistence;
using VendorManagementprojApplication.DTOs;

namespace VendorManagementprojApplication.Features.PurchaseRequests.Commands.UpdatePurchaseRequest;

public class UpdatePurchaseRequestCommandHandler
    : IRequestHandler<UpdatePurchaseRequestCommand, UpdatePurchaseRequestResponse>
{
    private readonly IPurchaseRequestRepository _repository;
    private readonly IUserRepository _userRepository;

    public UpdatePurchaseRequestCommandHandler(
        IPurchaseRequestRepository repository,
        IUserRepository userRepository)
    {
        _repository = repository;
        _userRepository = userRepository;
    }

    public async Task<UpdatePurchaseRequestResponse> Handle(
        UpdatePurchaseRequestCommand request,
        CancellationToken cancellationToken)
    {
        if (request.RequestID <= 0)
            throw new InvalidOperationException(
                "A valid purchase request is required.");

        if (request.OutletID <= 0)
            throw new InvalidOperationException(
                "A valid outlet is required.");

        if (request.UpdatedByUserID <= 0)
            throw new InvalidOperationException(
                "A valid user is required.");

        var purchaseRequest =
            await _repository.GetByIdAsync(
                request.RequestID);

        if (purchaseRequest == null)
        {
            return new UpdatePurchaseRequestResponse
            {
                PurchaseRequest = null
            };
        }

        if (!string.Equals(
            purchaseRequest.Status,
            "Pending",
            StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException(
                "Only a pending purchase request can be updated.");
        }

        var user =
            await _userRepository.GetByIdAsync(
                request.UpdatedByUserID);

        if (user == null)
            throw new InvalidOperationException(
                "User does not exist.");

        if (user.OutletID == null)
            throw new InvalidOperationException(
                "User is not assigned to an outlet.");

        if (user.OutletID != request.OutletID)
            throw new UnauthorizedAccessException(
                "User is not authorized to update a purchase request for this outlet.");

        purchaseRequest.OutletID =
            request.OutletID;

        var updated =
            await _repository.UpdateAsync(
                request.RequestID,
                purchaseRequest);

        if (updated == null)
        {
            return new UpdatePurchaseRequestResponse
            {
                PurchaseRequest = null
            };
        }

        return new UpdatePurchaseRequestResponse
        {
            PurchaseRequest = new PurchaseRequestDto
            {
                RequestID =
                    updated.RequestID,

                OutletID =
                    updated.OutletID,

                CreatedByUserID =
                    updated.CreatedByUserID,

                RequestDate =
                    updated.RequestDate,

                Status =
                    updated.Status
            }
        };
    }
}