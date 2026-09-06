using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using VendorManagementprojApplication.Contracts.Persistence;
using VendorManagementprojApplication.Contracts.Services;
using VendorManagementprojApplication.DTOs;

namespace VendorManagementprojApplication.Features.PurchaseRequests.Queries.GetPurchaseRequestById;

public class GetPurchaseRequestByIdQueryHandler : IRequestHandler<GetPurchaseRequestByIdQuery, GetPurchaseRequestByIdResponse>
{
    private readonly IPurchaseRequestRepository _repository;
    private readonly IOutletRepository _outletRepository;
    private readonly ICurrentUserService _currentUserService;

    public GetPurchaseRequestByIdQueryHandler(
        IPurchaseRequestRepository repository,
        IOutletRepository outletRepository,
        ICurrentUserService currentUserService)
    {
        _repository = repository;
        _outletRepository = outletRepository;
        _currentUserService = currentUserService;
    }

    public async Task<GetPurchaseRequestByIdResponse> Handle(GetPurchaseRequestByIdQuery request, CancellationToken cancellationToken)
    {
        var req = await _repository.GetByIdAsync(request.RequestID);

        if (req == null)
            return new GetPurchaseRequestByIdResponse { PurchaseRequest = null };

        if ((_currentUserService.IsOutletManager || _currentUserService.IsPurchaseManager) && _currentUserService.OutletID.HasValue)
        {
            if (req.OutletID != _currentUserService.OutletID.Value)
            {
                throw new UnauthorizedAccessException("You are not authorized to view purchase requests belonging to another outlet.");
            }
        }
        else if (_currentUserService.IsPurchaseManager && !_currentUserService.OutletID.HasValue)
        {
            throw new UnauthorizedAccessException("You are not assigned to an outlet.");
        }
        else if (_currentUserService.IsOrganizationManager && _currentUserService.OrganizationID.HasValue)
        {
            var orgOutlets = await _outletRepository.GetByOrganizationIdAsync(_currentUserService.OrganizationID.Value);
            var orgOutletIds = orgOutlets.Select(o => o.OutletID).ToHashSet();
            if (!orgOutletIds.Contains(req.OutletID))
            {
                throw new UnauthorizedAccessException("You are not authorized to view purchase requests outside your organization.");
            }
        }

        return new GetPurchaseRequestByIdResponse
        {
            PurchaseRequest = new PurchaseRequestDto
            {
                RequestID = req.RequestID,
                OutletID = req.OutletID,
                CreatedByUserID = req.CreatedByUserID,
                RequestDate = req.RequestDate,
                Status = req.Status,
                Items = req.Items?.Select(i => new PurchaseRequestItemDto
                {
                    RequestItemID = i.RequestItemID,
                    RequestID = i.RequestID,
                    ProductID = i.ProductID,
                    ProductName = i.Product?.ProductName ?? string.Empty,
                    Quantity = i.Quantity,
                    Unit = i.Unit
                }).ToList() ?? new List<PurchaseRequestItemDto>()
            }
        };
    }
}
