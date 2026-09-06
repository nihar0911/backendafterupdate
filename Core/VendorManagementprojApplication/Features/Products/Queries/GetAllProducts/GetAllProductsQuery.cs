using MediatR;
using VendorManagementprojApplication.DTOs;

namespace VendorManagementprojApplication.Features.Products.Queries.GetAllProducts;

public record GetAllProductsQuery : IRequest<GetAllProductsResponse>;
