using MediatR;

namespace VendorManagementprojApplication.Features.Organizations.Commands.DeleteOrganization;

public record DeleteOrganizationCommand(int OrganizationID) : IRequest<DeleteOrganizationResponse>;
