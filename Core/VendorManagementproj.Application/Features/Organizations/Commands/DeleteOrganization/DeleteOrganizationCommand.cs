using MediatR;

namespace VendorManagementproj.Application.Features.Organizations.Commands.DeleteOrganization;

public record DeleteOrganizationCommand(int OrganizationID) : IRequest<DeleteOrganizationResponse>;
