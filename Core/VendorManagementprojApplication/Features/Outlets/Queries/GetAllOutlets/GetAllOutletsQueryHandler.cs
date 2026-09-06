using MediatR;
using VendorManagementprojApplication.Contracts.Persistence;
using VendorManagementprojApplication.Contracts.Services;
using VendorManagementprojApplication.DTOs;

namespace VendorManagementprojApplication.Features.Outlets.Queries.GetAllOutlets;

public class GetAllOutletsQueryHandler
    : IRequestHandler<GetAllOutletsQuery, GetAllOutletsResponse>
{
    private readonly IOutletRepository _repository;
    private readonly ICurrentUserService _currentUserService;

    public GetAllOutletsQueryHandler(
        IOutletRepository repository,
        ICurrentUserService currentUserService)
    {
        _repository = repository;
        _currentUserService = currentUserService;
    }

    public async Task<GetAllOutletsResponse> Handle(
        GetAllOutletsQuery request,
        CancellationToken cancellationToken)
    {
        var outlets =
            await _repository.GetAllAsync();

        if (_currentUserService.IsAdmin)
        {
            return new GetAllOutletsResponse { Outlets = MapToDto(outlets) };
        }

        if (_currentUserService.IsOrganizationManager)
        {
            if (!_currentUserService.OrganizationID.HasValue)
                return new GetAllOutletsResponse { Outlets = new List<OutletDto>() };

            var organizationOutlets =
                outlets
                    .Where(o =>
                        o.OrganizationID ==
                        _currentUserService.OrganizationID.Value)
                    .ToList();

            return new GetAllOutletsResponse { Outlets = MapToDto(organizationOutlets) };
        }

        if (_currentUserService.IsOutletManager)
        {
            if (!_currentUserService.OutletID.HasValue)
                return new GetAllOutletsResponse { Outlets = new List<OutletDto>() };

            var outlet =
                outlets
                    .Where(o =>
                        o.OutletID ==
                        _currentUserService.OutletID.Value)
                    .ToList();

            return new GetAllOutletsResponse { Outlets = MapToDto(outlet) };
        }

        return new GetAllOutletsResponse { Outlets = new List<OutletDto>() };
    }

    private static List<OutletDto> MapToDto(
        IEnumerable<VendorManagementprojDomain.Entities.Outlet> outlets)
    {
        return outlets.Select(o => new OutletDto
        {
            OutletID = o.OutletID,
            OrganizationID = o.OrganizationID,
            OrganizationName =
                o.Organization?.OrganizationName ?? string.Empty,
            OutletName = o.OutletName,
            Address = o.Address,
            Latitude = o.Latitude,
            Longitude = o.Longitude
        }).ToList();
    }
}