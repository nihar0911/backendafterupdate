using MediatR;
using VendorManagementprojApplication.Contracts.Persistence;
using VendorManagementprojApplication.Contracts.Services;
using VendorManagementprojApplication.DTOs;

namespace VendorManagementprojApplication.Features.Organizations.Queries.GetAllOrganizations;

public class GetAllOrganizationsQueryHandler
    : IRequestHandler<GetAllOrganizationsQuery, GetAllOrganizationsResponse>
{
    private readonly IOrganizationRepository _repository;
    private readonly IOutletRepository _outletRepository;
    private readonly ICurrentUserService _currentUserService;

    public GetAllOrganizationsQueryHandler(
        IOrganizationRepository repository,
        IOutletRepository outletRepository,
        ICurrentUserService currentUserService)
    {
        _repository = repository;
        _outletRepository = outletRepository;
        _currentUserService = currentUserService;
    }

    public async Task<GetAllOrganizationsResponse> Handle(
        GetAllOrganizationsQuery request,
        CancellationToken cancellationToken)
    {
        var organizations = await _repository.GetAllAsync();

        if (_currentUserService.IsAdmin)
        {
            var list = organizations.Select(organization => new OrganizationDto
            {
                OrganizationID = organization.OrganizationID,
                OrganizationName = organization.OrganizationName,
                Address = organization.Address,
                Phone = organization.Phone,
                Email = organization.Email
            }).ToList();

            return new GetAllOrganizationsResponse { Organizations = list };
        }

        if (_currentUserService.IsOrganizationManager)
        {
            if (!_currentUserService.OrganizationID.HasValue)
            {
                return new GetAllOrganizationsResponse { Organizations = new List<OrganizationDto>() };
            }

            organizations = organizations
                .Where(organization =>
                    organization.OrganizationID ==
                    _currentUserService.OrganizationID.Value)
                .ToList();

            var list = organizations.Select(organization => new OrganizationDto
            {
                OrganizationID = organization.OrganizationID,
                OrganizationName = organization.OrganizationName,
                Address = organization.Address,
                Phone = organization.Phone,
                Email = organization.Email
            }).ToList();

            return new GetAllOrganizationsResponse { Organizations = list };
        }

        if (_currentUserService.IsOutletManager)
        {
            int? targetOrgId = _currentUserService.OrganizationID;

            if (!targetOrgId.HasValue && _currentUserService.OutletID.HasValue)
            {
                var outlet = await _outletRepository.GetByIdAsync(_currentUserService.OutletID.Value);
                if (outlet != null)
                {
                    targetOrgId = outlet.OrganizationID;
                }
            }

            if (!targetOrgId.HasValue)
            {
                return new GetAllOrganizationsResponse { Organizations = new List<OrganizationDto>() };
            }

            organizations = organizations
                .Where(organization => organization.OrganizationID == targetOrgId.Value)
                .ToList();

            var list = organizations.Select(organization => new OrganizationDto
            {
                OrganizationID = organization.OrganizationID,
                OrganizationName = organization.OrganizationName,
                Address = organization.Address,
                Phone = organization.Phone,
                Email = organization.Email
            }).ToList();

            return new GetAllOrganizationsResponse { Organizations = list };
        }

        return new GetAllOrganizationsResponse { Organizations = new List<OrganizationDto>() };
    }
}