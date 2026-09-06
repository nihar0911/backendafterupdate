using VendorManagementprojApplication.DTOs;

namespace VendorManagementprojApplication.Features.Organizations.Queries.GetAllOrganizations;

public class GetAllOrganizationsResponse
{
    public List<OrganizationDto> Organizations { get; set; } = new();
}
