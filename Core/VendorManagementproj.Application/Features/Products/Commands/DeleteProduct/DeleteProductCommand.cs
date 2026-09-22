using MediatR;

namespace VendorManagementproj.Application.Features.Products.Commands.DeleteProduct;

public record DeleteProductCommand(int ProductID) : IRequest<DeleteProductResponse>;
