using MediatR;
using VendorManagementprojApplication.DTOs;

namespace VendorManagementprojApplication.Features.Vendors.Queries.GetVendorById;

public record GetVendorByIdQuery(int VendorID) : IRequest<GetVendorByIdResponse>;
