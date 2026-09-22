using MediatR;
using VendorManagementproj.Application.DTOs;

namespace VendorManagementproj.Application.Features.Outlets.Queries.GetOutletsByOrganizationId;

public record GetOutletsByOrganizationIdQuery(int OrganizationID) : IRequest<GetOutletsByOrganizationIdResponse>;
