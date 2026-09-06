using MediatR;
using VendorManagementprojApplication.DTOs;

namespace VendorManagementprojApplication.Features.Products.Queries.GetProductById;

public record GetProductByIdQuery(int ProductID) : IRequest<GetProductByIdResponse>;
