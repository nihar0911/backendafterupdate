using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using VendorManagementprojApplication.Contracts.Persistence;
using VendorManagementprojApplication.Contracts.Services;
using VendorManagementprojApplication.DTOs;
using VendorManagementprojDomain.Entities;

namespace VendorManagementprojApplication.Features.PurchaseRequests.Queries.GetAllPurchaseRequests;

public class GetAllPurchaseRequestsQueryHandler : IRequestHandler<GetAllPurchaseRequestsQuery, GetAllPurchaseRequestsResponse>
{
    private readonly IPurchaseRequestRepository _repository;
    private readonly IOutletRepository _outletRepository;
    private readonly ICurrentUserService _currentUserService;

    public GetAllPurchaseRequestsQueryHandler(
        IPurchaseRequestRepository repository,
        IOutletRepository outletRepository,
        ICurrentUserService currentUserService)
    {
        _repository = repository;
        _outletRepository = outletRepository;
        _currentUserService = currentUserService;
    }

    public async Task<GetAllPurchaseRequestsResponse> Handle(GetAllPurchaseRequestsQuery request, CancellationToken cancellationToken)
    {
        var requests = await _repository.GetAllAsync();

        if (_currentUserService.IsOrganizationManager)
        {
            if (_currentUserService.OrganizationID.HasValue)
            {
                var orgOutlets = await _outletRepository.GetByOrganizationIdAsync(_currentUserService.OrganizationID.Value);
                var orgOutletIds = orgOutlets.Select(o => o.OutletID).ToHashSet();
                requests = requests.Where(r => orgOutletIds.Contains(r.OutletID)).ToList();
            }
            else
            {
                requests = new List<PurchaseRequest>();
            }
        }
        else if (_currentUserService.IsOutletManager || _currentUserService.IsPurchaseManager)
        {
            if (_currentUserService.OutletID.HasValue)
            {
                requests = requests.Where(r => r.OutletID == _currentUserService.OutletID.Value).ToList();
            }
            else
            {
                requests = new List<PurchaseRequest>();
            }
        }

        var list = requests.Select(req => new PurchaseRequestDto
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
        }).ToList();

        return new GetAllPurchaseRequestsResponse
        {
            PurchaseRequests = list
        };
    }
}
