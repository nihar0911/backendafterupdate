using MediatR;
using VendorManagementproj.Domain.Entities;

namespace VendorManagementproj.Application.Features.VendorProducts.Queries.GetVendorProductById;

public record GetVendorProductByIdQuery(int VendorProductID) : IRequest<GetVendorProductByIdResponse>;
