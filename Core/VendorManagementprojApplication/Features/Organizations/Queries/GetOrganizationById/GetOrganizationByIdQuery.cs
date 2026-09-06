using MediatR;
using VendorManagementprojApplication.DTOs;

namespace VendorManagementprojApplication.Features.Organizations.Queries.GetOrganizationById;

public record GetOrganizationByIdQuery(int OrganizationID) : IRequest<GetOrganizationByIdResponse>;
