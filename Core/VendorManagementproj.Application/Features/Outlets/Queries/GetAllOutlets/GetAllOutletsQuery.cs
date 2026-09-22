using MediatR;
using VendorManagementproj.Application.DTOs;

namespace VendorManagementproj.Application.Features.Outlets.Queries.GetAllOutlets;

public record GetAllOutletsQuery : IRequest<GetAllOutletsResponse>;
