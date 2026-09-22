using MediatR;
using VendorManagementprojApplication.DTOs;

namespace VendorManagementprojApplication.Features.Vendors.Queries.GetAllVendors;

public record GetAllVendorsQuery : IRequest<GetAllVendorsResponse>;
