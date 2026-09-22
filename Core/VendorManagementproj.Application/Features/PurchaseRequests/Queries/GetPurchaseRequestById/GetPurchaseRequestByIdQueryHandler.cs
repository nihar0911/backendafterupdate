using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using VendorManagementproj.Application.Contracts.Persistence;
using VendorManagementproj.Application.Contracts.Services;
using VendorManagementproj.Application.DTOs;

namespace VendorManagementproj.Application.Features.PurchaseRequests.Queries.GetPurchaseRequestById;

public class GetPurchaseRequestByIdQueryHandler : IRequestHandler<GetPurchaseRequestByIdQuery, GetPurchaseRequestByIdResponse>
{
    private readonly IPurchaseRequestRepository _repository;
    private readonly IOutletRepository _outletRepository;
    private readonly IVendorOpportunityResponseRepository _responseRepository;
    private readonly IVendorRepository _vendorRepository;
    private readonly ICurrentUserService _currentUserService;

    public GetPurchaseRequestByIdQueryHandler(
        IPurchaseRequestRepository repository,
        IOutletRepository outletRepository,
        IVendorOpportunityResponseRepository responseRepository,
        IVendorRepository vendorRepository,
        ICurrentUserService currentUserService)
    {
        _repository = repository;
        _outletRepository = outletRepository;
        _responseRepository = responseRepository;
        _vendorRepository = vendorRepository;
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

        string? rejectionReason = null;
        string? vendorName = null;

        var responses = await _responseRepository.GetByRequestIdAsync(req.RequestID);
        if (responses != null && responses.Count > 0)
        {
            var rejectedResponse = responses.FirstOrDefault(r => string.Equals(r.Status, "Rejected", StringComparison.OrdinalIgnoreCase));
            if (rejectedResponse != null)
            {
                rejectionReason = rejectedResponse.RejectionReason;
                var vendor = await _vendorRepository.GetByIdAsync(rejectedResponse.VendorID);
                if (vendor != null)
                {
                    vendorName = vendor.VendorName;
                }
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
                VendorName = vendorName,
                RejectionReason = rejectionReason,
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