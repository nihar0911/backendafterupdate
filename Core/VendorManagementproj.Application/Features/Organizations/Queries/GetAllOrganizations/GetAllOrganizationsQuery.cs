using MediatR;
using VendorManagementproj.Application.DTOs;

namespace VendorManagementproj.Application.Features.Organizations.Queries.GetAllOrganizations;

public record GetAllOrganizationsQuery : IRequest<GetAllOrganizationsResponse>;
