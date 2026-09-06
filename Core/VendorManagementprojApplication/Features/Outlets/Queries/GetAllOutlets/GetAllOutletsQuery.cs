using MediatR;
using VendorManagementprojApplication.DTOs;

namespace VendorManagementprojApplication.Features.Outlets.Queries.GetAllOutlets;

public record GetAllOutletsQuery : IRequest<GetAllOutletsResponse>;
