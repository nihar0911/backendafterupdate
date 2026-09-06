using MediatR;
using VendorManagementprojApplication.Contracts.Persistence;
using VendorManagementprojApplication.Contracts.Services;
using VendorManagementprojApplication.DTOs;

namespace VendorManagementprojApplication.Features.Outlets.Queries.GetOutletById;

public class GetOutletByIdQueryHandler
    : IRequestHandler<GetOutletByIdQuery, GetOutletByIdResponse>
{
    private readonly IOutletRepository _repository;
    private readonly ICurrentUserService _currentUserService;

    public GetOutletByIdQueryHandler(
        IOutletRepository repository,
        ICurrentUserService currentUserService)
    {
        _repository = repository;
        _currentUserService = currentUserService;
    }

    public async Task<GetOutletByIdResponse> Handle(
        GetOutletByIdQuery request,
        CancellationToken cancellationToken)
    {
        var outlet =
            await _repository.GetByIdAsync(
                request.OutletID);

        if (outlet == null)
            return new GetOutletByIdResponse { Outlet = null };

        if (_currentUserService.IsAdmin)
        {
            return new GetOutletByIdResponse { Outlet = MapToDto(outlet) };
        }

        if (_currentUserService.IsOrganizationManager)
        {
            if (!_currentUserService.OrganizationID.HasValue)
                return new GetOutletByIdResponse { Outlet = null };

            if (outlet.OrganizationID !=
                _currentUserService.OrganizationID.Value)
            {
                return new GetOutletByIdResponse { Outlet = null };
            }

            return new GetOutletByIdResponse { Outlet = MapToDto(outlet) };
        }

        if (_currentUserService.IsOutletManager)
        {
            if (!_currentUserService.OrganizationID.HasValue ||
                !_currentUserService.OutletID.HasValue)
            {
                return new GetOutletByIdResponse { Outlet = null };
            }

            if (outlet.OrganizationID !=
                _currentUserService.OrganizationID.Value)
            {
                return new GetOutletByIdResponse { Outlet = null };
            }

            if (outlet.OutletID !=
                _currentUserService.OutletID.Value)
            {
                return new GetOutletByIdResponse { Outlet = null };
            }

            return new GetOutletByIdResponse { Outlet = MapToDto(outlet) };
        }

        return new GetOutletByIdResponse { Outlet = null };
    }

    private static OutletDto MapToDto(
        VendorManagementprojDomain.Entities.Outlet outlet)
    {
        return new OutletDto
        {
            OutletID = outlet.OutletID,
            OrganizationID = outlet.OrganizationID,
            OrganizationName =
                outlet.Organization?.OrganizationName ?? string.Empty,
            OutletName = outlet.OutletName,
            Address = outlet.Address,
            Latitude = outlet.Latitude,
            Longitude = outlet.Longitude,
            PurchaseOrderApproverRole = outlet.PurchaseOrderApproverRole
        };
    }
}