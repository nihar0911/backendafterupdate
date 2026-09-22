using MediatR;
using VendorManagementproj.Domain.Entities;

namespace VendorManagementproj.Application.Features.VendorProducts.Queries.GetAllVendorProducts;

public record GetAllVendorProductsQuery : IRequest<GetAllVendorProductsResponse>;
