using MediatR;
using VendorManagementprojApplication.DTOs;

namespace VendorManagementprojApplication.Features.Outlets.Queries.GetOutletsByOrganizationId;

public record GetOutletsByOrganizationIdQuery(int OrganizationID) : IRequest<GetOutletsByOrganizationIdResponse>;
