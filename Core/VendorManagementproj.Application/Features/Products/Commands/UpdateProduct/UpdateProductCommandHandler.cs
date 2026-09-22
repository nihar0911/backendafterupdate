using MediatR;
using VendorManagementproj.Application.Contracts.Persistence;
using VendorManagementproj.Application.DTOs;
using VendorManagementproj.Domain.Entities;

namespace VendorManagementproj.Application.Features.Products.Commands.UpdateProduct;

public class UpdateProductCommandHandler : IRequestHandler<UpdateProductCommand, UpdateProductResponse>
{
    private readonly IProductRepository _productRepository;

    public UpdateProductCommandHandler(IProductRepository productRepository)
    {
        _productRepository = productRepository;
    }

    public async Task<UpdateProductResponse> Handle(UpdateProductCommand request, CancellationToken cancellationToken)
    {
        var product = new Product
        {
            ProductName = request.ProductName,
            Category = request.Category,
            Unit = request.Unit,
            TaxRateID = request.TaxRateID,
            Status = request.Status
        };

        var updatedProduct = await _productRepository.UpdateAsync(request.ProductID, product);

        if (updatedProduct == null)
            return new UpdateProductResponse { Product = null };

        return new UpdateProductResponse
        {
            Product = new ProductDto
            {
                ProductID = updatedProduct.ProductID,
                ProductName = updatedProduct.ProductName,
                Category = updatedProduct.Category,
                Unit = updatedProduct.Unit,
                TaxRateID = updatedProduct.TaxRateID,
                Status = updatedProduct.Status
            }
        };
    }
}
