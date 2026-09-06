using MediatR;

namespace VendorManagementprojApplication.Features.Products.Commands.DeleteProduct;

public record DeleteProductCommand(int ProductID) : IRequest<DeleteProductResponse>;
