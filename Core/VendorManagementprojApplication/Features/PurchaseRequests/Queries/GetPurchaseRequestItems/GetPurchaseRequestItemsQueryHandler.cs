using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using VendorManagementprojApplication.Contracts.Persistence;
using VendorManagementprojApplication.Contracts.Services;
using VendorManagementprojApplication.DTOs;

namespace VendorManagementprojApplication.Features.PurchaseRequests.Queries.GetPurchaseRequestItems;

public class GetPurchaseRequestItemsQueryHandler
    : IRequestHandler<GetPurchaseRequestItemsQuery, GetPurchaseRequestItemsResponse>
{
    private readonly IPurchaseRequestRepository _repository;
    private readonly IOutletRepository _outletRepository;
    private readonly ICurrentUserService _currentUserService;

    public GetPurchaseRequestItemsQueryHandler(
        IPurchaseRequestRepository repository,
        IOutletRepository outletRepository,
        ICurrentUserService currentUserService)
    {
        _repository = repository;
        _outletRepository = outletRepository;
        _currentUserService = currentUserService;
    }

    public async Task<GetPurchaseRequestItemsResponse> Handle(
        GetPurchaseRequestItemsQuery request,
        CancellationToken cancellationToken)
    {
        var purchaseRequest = await _repository.GetByIdAsync(request.RequestID);
        if (purchaseRequest == null)
        {
            return new GetPurchaseRequestItemsResponse { Items = new List<PurchaseRequestItemDto>() };
        }

        if ((_currentUserService.IsOutletManager || _currentUserService.IsPurchaseManager) && _currentUserService.OutletID.HasValue)
        {
            if (purchaseRequest.OutletID != _currentUserService.OutletID.Value)
            {
                throw new UnauthorizedAccessException("You are not authorized to view purchase request items belonging to another outlet.");
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
            if (!orgOutletIds.Contains(purchaseRequest.OutletID))
            {
                throw new UnauthorizedAccessException("You are not authorized to view purchase request items outside your organization.");
            }
        }

        var items = await _repository.GetItemsByRequestIdAsync(
            request.RequestID);

        var list = items.Select(i => new PurchaseRequestItemDto
        {
            RequestItemID = i.RequestItemID,
            RequestID = i.RequestID,
            ProductID = i.ProductID,
            ProductName = i.Product?.ProductName
                ?? $"Product #{i.ProductID}",
            Quantity = i.Quantity,
            Unit = i.Unit
        }).ToList();

        return new GetPurchaseRequestItemsResponse
        {
            Items = list
        };
    }
}