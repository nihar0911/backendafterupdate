using MediatR;
using VendorManagementproj.Application.DTOs;

namespace VendorManagementproj.Application.Features.Products.Queries.GetAllProducts;

public record GetAllProductsQuery : IRequest<GetAllProductsResponse>;
