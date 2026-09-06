using MediatR;
using VendorManagementprojApplication.Contracts.Persistence;
using VendorManagementprojApplication.Contracts.Services;
using VendorManagementprojApplication.DTOs;
using VendorManagementprojDomain.Entities;

namespace VendorManagementprojApplication.Features.Outlets.Commands.CreateOutlet;

public class CreateOutletCommandHandler
    : IRequestHandler<CreateOutletCommand, CreateOutletResponse>
{
    private readonly IOutletRepository _repository;
    private readonly ICurrentUserService _currentUserService;

    public CreateOutletCommandHandler(
        IOutletRepository repository,
        ICurrentUserService currentUserService)
    {
        _repository = repository;
        _currentUserService = currentUserService;
    }

    public async Task<CreateOutletResponse> Handle(
        CreateOutletCommand request,
        CancellationToken cancellationToken)
    {
        int organizationID;

        if (_currentUserService.IsAdmin)
        {
            organizationID = request.OrganizationID;
        }
        else if (_currentUserService.IsOrganizationManager)
        {
            if (!_currentUserService.OrganizationID.HasValue)
            {
                throw new UnauthorizedAccessException(
                    "User is not assigned to an organization.");
            }

            organizationID =
                _currentUserService.OrganizationID.Value;

            if (request.OrganizationID != organizationID)
            {
                throw new UnauthorizedAccessException(
                    "You cannot create an outlet for another organization.");
            }
        }
        else
        {
            throw new UnauthorizedAccessException(
                "You are not authorized to create an outlet.");
        }

        var outlet = new Outlet
        {
            OrganizationID = organizationID,
            OutletName = request.OutletName,
            Address = request.Address,
            Latitude = request.Latitude,
            Longitude = request.Longitude
        };

        var created =
            await _repository.AddAsync(outlet);

        var dto = new OutletDto
        {
            OutletID = created.OutletID,
            OrganizationID = created.OrganizationID,
            OutletName = created.OutletName,
            Address = created.Address,
            Latitude = created.Latitude,
            Longitude = created.Longitude
        };

        return new CreateOutletResponse
        {
            Outlet = dto
        };
    }
}