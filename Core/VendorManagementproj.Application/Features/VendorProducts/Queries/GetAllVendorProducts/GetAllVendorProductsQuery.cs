using MediatR;
using VendorManagementprojDomain.Entities;

namespace VendorManagementprojApplication.Features.VendorProducts.Queries.GetAllVendorProducts;

public record GetAllVendorProductsQuery : IRequest<GetAllVendorProductsResponse>;
