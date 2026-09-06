using MediatR;
using VendorManagementprojApplication.Contracts.Persistence;
using VendorManagementprojApplication.Contracts.Services;
using VendorManagementprojApplication.DTOs;

namespace VendorManagementprojApplication.Features.Organizations.Queries.GetOrganizationById;

public class GetOrganizationByIdQueryHandler
    : IRequestHandler<GetOrganizationByIdQuery, GetOrganizationByIdResponse>
{
    private readonly IOrganizationRepository _repository;
    private readonly ICurrentUserService _currentUserService;

    public GetOrganizationByIdQueryHandler(
        IOrganizationRepository repository,
        ICurrentUserService currentUserService)
    {
        _repository = repository;
        _currentUserService = currentUserService;
    }

    public async Task<GetOrganizationByIdResponse> Handle(
        GetOrganizationByIdQuery request,
        CancellationToken cancellationToken)
    {
        var organization =
            await _repository.GetByIdAsync(
                request.OrganizationID);

        if (organization == null)
            return new GetOrganizationByIdResponse { Organization = null };

        if (_currentUserService.IsAdmin)
        {
            return new GetOrganizationByIdResponse { Organization = MapToDto(organization) };
        }

        if (_currentUserService.IsOrganizationManager)
        {
            if (!_currentUserService.OrganizationID.HasValue)
                return new GetOrganizationByIdResponse { Organization = null };

            if (organization.OrganizationID !=
                _currentUserService.OrganizationID.Value)
            {
                return new GetOrganizationByIdResponse { Organization = null };
            }

            return new GetOrganizationByIdResponse { Organization = MapToDto(organization) };
        }

        if (_currentUserService.IsOutletManager)
        {
            return new GetOrganizationByIdResponse { Organization = MapToDto(organization) };
        }

        return new GetOrganizationByIdResponse { Organization = null };
    }

    private static OrganizationDto MapToDto(
        VendorManagementprojDomain.Entities.Organization organization)
    {
        return new OrganizationDto
        {
            OrganizationID = organization.OrganizationID,
            OrganizationName = organization.OrganizationName,
            Address = organization.Address,
            Phone = organization.Phone,
            Email = organization.Email
        };
    }
}