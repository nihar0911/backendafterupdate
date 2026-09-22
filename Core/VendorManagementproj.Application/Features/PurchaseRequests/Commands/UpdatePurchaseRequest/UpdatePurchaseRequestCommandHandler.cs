using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using VendorManagementprojApplication.Contracts.Persistence;
using VendorManagementprojApplication.Contracts.Services;
using VendorManagementprojApplication.DTOs;

namespace VendorManagementprojApplication.Features.PurchaseRequests.Commands.UpdatePurchaseRequest;

public class UpdatePurchaseRequestCommandHandler
    : IRequestHandler<UpdatePurchaseRequestCommand, UpdatePurchaseRequestResponse>
{
    private readonly IPurchaseRequestRepository _repository;
    private readonly IUserRepository _userRepository;
    private readonly ICurrentUserService _currentUserService;

    public UpdatePurchaseRequestCommandHandler(
        IPurchaseRequestRepository repository,
        IUserRepository userRepository,
        ICurrentUserService currentUserService)
    {
        _repository = repository;
        _userRepository = userRepository;
        _currentUserService = currentUserService;
    }

    public async Task<UpdatePurchaseRequestResponse> Handle(
        UpdatePurchaseRequestCommand request,
        CancellationToken cancellationToken)
    {
        if (request.RequestID <= 0)
            throw new InvalidOperationException("A valid purchase request is required.");

        if (request.OutletID <= 0)
            throw new InvalidOperationException("A valid outlet is required.");

        var purchaseRequest = await _repository.GetByIdAsync(request.RequestID);

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
            throw new InvalidOperationException("Only a pending purchase request can be updated.");
        }

        bool isAuthorized = false;
        if (_currentUserService.IsAdmin)
        {
            isAuthorized = true;
        }
        else if (_currentUserService.IsPurchaseManager && _currentUserService.OutletID.HasValue)
        {
            if (_currentUserService.OutletID.Value == request.OutletID && _currentUserService.OutletID.Value == purchaseRequest.OutletID)
            {
                isAuthorized = true;
            }
        }
        else if (request.UpdatedByUserID > 0)
        {
            var user = await _userRepository.GetByIdAsync(request.UpdatedByUserID);
            if (user != null)
            {
                if (string.Equals(user.Role?.RoleName, "Admin", StringComparison.OrdinalIgnoreCase))
                {
                    isAuthorized = true;
                }
                else if (string.Equals(user.Role?.RoleName, "Purchase Manager", StringComparison.OrdinalIgnoreCase) && user.OutletID == request.OutletID && user.OutletID == purchaseRequest.OutletID)
                {
                    isAuthorized = true;
                }
            }
        }

        if (!isAuthorized)
        {
            throw new UnauthorizedAccessException("User is not authorized to update this purchase request.");
        }

        purchaseRequest.OutletID = request.OutletID;

        var updated = await _repository.UpdateAsync(request.RequestID, purchaseRequest);

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
                RequestID = updated.RequestID,
                OutletID = updated.OutletID,
                CreatedByUserID = updated.CreatedByUserID,
                RequestDate = updated.RequestDate,
                Status = updated.Status
            }
        };
    }
}