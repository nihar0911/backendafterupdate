using MediatR;
using VendorManagementprojApplication.DTOs;

namespace VendorManagementprojApplication.Features.Organizations.Commands.UpdateOrganization;

public class UpdateOrganizationCommand : IRequest<UpdateOrganizationResponse>
{
    public int OrganizationID { get; set; }
    public string OrganizationName { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
}
