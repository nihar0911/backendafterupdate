using MediatR;
using VendorManagementproj.Application.DTOs;

namespace VendorManagementproj.Application.Features.Organizations.Queries.GetOrganizationById;

public record GetOrganizationByIdQuery(int OrganizationID) : IRequest<GetOrganizationByIdResponse>;
