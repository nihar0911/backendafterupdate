using MediatR;
using VendorManagementprojApplication.Contracts.Persistence;

namespace VendorManagementprojApplication.Features.Products.Commands.DeleteProduct;

public class DeleteProductCommandHandler : IRequestHandler<DeleteProductCommand, DeleteProductResponse>
{
    private readonly IProductRepository _productRepository;

    public DeleteProductCommandHandler(IProductRepository productRepository)
    {
        _productRepository = productRepository;
    }

    public async Task<DeleteProductResponse> Handle(DeleteProductCommand request, CancellationToken cancellationToken)
    {
        var success = await _productRepository.DeleteAsync(request.ProductID);
        return new DeleteProductResponse
        {
            Success = success
        };
    }
}
