using VendorManagementprojApplication.DTOs;

namespace VendorManagementprojApplication.Features.Organizations.Commands.CreateOrganization;

public class CreateOrganizationResponse
{
    public OrganizationDto Organization { get; set; } = null!;
}
