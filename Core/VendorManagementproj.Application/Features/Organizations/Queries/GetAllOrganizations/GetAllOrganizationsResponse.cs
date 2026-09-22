using VendorManagementproj.Application.DTOs;

namespace VendorManagementproj.Application.Features.Organizations.Queries.GetAllOrganizations;

public class GetAllOrganizationsResponse
{
    public List<OrganizationDto> Organizations { get; set; } = new();
}
