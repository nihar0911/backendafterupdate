using VendorManagementproj.Application.DTOs;

namespace VendorManagementproj.Application.Features.Organizations.Commands.CreateOrganization;

public class CreateOrganizationResponse
{
    public OrganizationDto Organization { get; set; } = null!;
}
