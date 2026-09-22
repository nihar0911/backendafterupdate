using MediatR;
using VendorManagementproj.Application.DTOs;

namespace VendorManagementproj.Application.Features.Vendors.Queries.GetAllVendors;

public record GetAllVendorsQuery : IRequest<GetAllVendorsResponse>;
