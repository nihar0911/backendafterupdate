using MediatR;
using VendorManagementproj.Application.DTOs;

namespace VendorManagementproj.Application.Features.Vendors.Queries.GetVendorById;

public record GetVendorByIdQuery(int VendorID) : IRequest<GetVendorByIdResponse>;
