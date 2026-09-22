using MediatR;
using VendorManagementprojApplication.DTOs;

namespace VendorManagementprojApplication.Features.Organizations.Queries.GetAllOrganizations;

public record GetAllOrganizationsQuery : IRequest<GetAllOrganizationsResponse>;
