using MediatR;
using VendorManagementprojApplication.Contracts.Persistence;
using VendorManagementprojApplication.Contracts.Services;
using VendorManagementprojApplication.DTOs;

namespace VendorManagementprojApplication.Features.Outlets.Queries.GetOutletsByOrganizationId;

public class GetOutletsByOrganizationIdQueryHandler
    : IRequestHandler<GetOutletsByOrganizationIdQuery, GetOutletsByOrganizationIdResponse>
{
    private readonly IOutletRepository _repository;
    private readonly ICurrentUserService _currentUserService;

    public GetOutletsByOrganizationIdQueryHandler(
        IOutletRepository repository,
        ICurrentUserService currentUserService)
    {
        _repository = repository;
        _currentUserService = currentUserService;
    }

    public async Task<GetOutletsByOrganizationIdResponse> Handle(
        GetOutletsByOrganizationIdQuery request,
        CancellationToken cancellationToken)
    {
        if (_currentUserService.IsAdmin)
        {
            var adminOutlets =
                await _repository.GetByOrganizationIdAsync(
                    request.OrganizationID);

            return new GetOutletsByOrganizationIdResponse { Outlets = MapToDto(adminOutlets) };
        }

        if (_currentUserService.IsOrganizationManager)
        {
            if (!_currentUserService.OrganizationID.HasValue)
                return new GetOutletsByOrganizationIdResponse { Outlets = new List<OutletDto>() };

            if (request.OrganizationID !=
                _currentUserService.OrganizationID.Value)
            {
                return new GetOutletsByOrganizationIdResponse { Outlets = new List<OutletDto>() };
            }

            var organizationOutlets =
                await _repository.GetByOrganizationIdAsync(
                    _currentUserService.OrganizationID.Value);

            return new GetOutletsByOrganizationIdResponse { Outlets = MapToDto(organizationOutlets) };
        }

        return new GetOutletsByOrganizationIdResponse { Outlets = new List<OutletDto>() };
    }

    private static List<OutletDto> MapToDto(
        IEnumerable<VendorManagementprojDomain.Entities.Outlet> outlets)
    {
        return outlets.Select(o => new OutletDto
        {
            OutletID = o.OutletID,
            OrganizationID = o.OrganizationID,
            OutletName = o.OutletName,
            Address = o.Address,
            Latitude = o.Latitude,
            Longitude = o.Longitude
        }).ToList();
    }
}