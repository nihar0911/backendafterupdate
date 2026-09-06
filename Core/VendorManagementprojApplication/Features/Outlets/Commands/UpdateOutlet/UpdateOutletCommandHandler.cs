using MediatR;
using VendorManagementprojApplication.Contracts.Persistence;
using VendorManagementprojApplication.Contracts.Services;
using VendorManagementprojApplication.DTOs;

namespace VendorManagementprojApplication.Features.Outlets.Commands.UpdateOutlet;

public class UpdateOutletCommandHandler
    : IRequestHandler<UpdateOutletCommand, UpdateOutletResponse>
{
    private readonly IOutletRepository _repository;
    private readonly ICurrentUserService _currentUserService;

    public UpdateOutletCommandHandler(
        IOutletRepository repository,
        ICurrentUserService currentUserService)
    {
        _repository = repository;
        _currentUserService = currentUserService;
    }

    public async Task<UpdateOutletResponse> Handle(
        UpdateOutletCommand request,
        CancellationToken cancellationToken)
    {
        var outlet =
            await _repository.GetByIdAsync(
                request.OutletID);

        if (outlet == null)
            return new UpdateOutletResponse { Outlet = null };

        if (_currentUserService.IsAdmin)
        {
            outlet.OrganizationID =
                request.OrganizationID;
        }
        else if (_currentUserService.IsOrganizationManager)
        {
            if (!_currentUserService.OrganizationID.HasValue)
                return new UpdateOutletResponse { Outlet = null };

            if (outlet.OrganizationID !=
                _currentUserService.OrganizationID.Value)
            {
                return new UpdateOutletResponse { Outlet = null };
            }

            if (request.OrganizationID !=
                _currentUserService.OrganizationID.Value)
            {
                throw new UnauthorizedAccessException(
                    "You cannot move an outlet to another organization.");
            }

            outlet.OrganizationID =
                _currentUserService.OrganizationID.Value;
        }
        else
        {
            return new UpdateOutletResponse { Outlet = null };
        }

        outlet.OutletName = request.OutletName;
        outlet.Address = request.Address;
        outlet.Latitude = request.Latitude;
        outlet.Longitude = request.Longitude;

        var updated =
            await _repository.UpdateAsync(outlet);

        var dto = new OutletDto
        {
            OutletID = updated.OutletID,
            OrganizationID = updated.OrganizationID,
            OutletName = updated.OutletName,
            Address = updated.Address,
            Latitude = updated.Latitude,
            Longitude = updated.Longitude
        };

        return new UpdateOutletResponse
        {
            Outlet = dto
        };
    }
}