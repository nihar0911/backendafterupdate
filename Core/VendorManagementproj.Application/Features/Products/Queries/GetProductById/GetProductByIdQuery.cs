using MediatR;
using VendorManagementproj.Application.DTOs;

namespace VendorManagementproj.Application.Features.Products.Queries.GetProductById;

public record GetProductByIdQuery(int ProductID) : IRequest<GetProductByIdResponse>;
