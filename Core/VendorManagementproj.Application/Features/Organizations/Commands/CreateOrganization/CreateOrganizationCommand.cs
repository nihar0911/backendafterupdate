using MediatR;
using VendorManagementprojApplication.DTOs;

namespace VendorManagementprojApplication.Features.Organizations.Commands.CreateOrganization;

public class CreateOrganizationCommand : IRequest<CreateOrganizationResponse>
{
    public string OrganizationName { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
}
