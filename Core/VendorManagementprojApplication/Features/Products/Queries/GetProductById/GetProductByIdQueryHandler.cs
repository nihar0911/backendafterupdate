using MediatR;
using VendorManagementprojApplication.Contracts.Persistence;
using VendorManagementprojApplication.DTOs;

namespace VendorManagementprojApplication.Features.Products.Queries.GetProductById;

public class GetProductByIdQueryHandler : IRequestHandler<GetProductByIdQuery, GetProductByIdResponse>
{
    private readonly IProductRepository _productRepository;

    public GetProductByIdQueryHandler(IProductRepository productRepository)
    {
        _productRepository = productRepository;
    }

    public async Task<GetProductByIdResponse> Handle(GetProductByIdQuery request, CancellationToken cancellationToken)
    {
        var product = await _productRepository.GetByIdAsync(request.ProductID);

        if (product == null)
            return new GetProductByIdResponse { Product = null };

        return new GetProductByIdResponse
        {
            Product = new ProductDto
            {
                ProductID = product.ProductID,
                ProductName = product.ProductName,
                Category = product.Category,
                Unit = product.Unit,
                TaxRateID = product.TaxRateID,
                Status = product.Status
            }
        };
    }
}
