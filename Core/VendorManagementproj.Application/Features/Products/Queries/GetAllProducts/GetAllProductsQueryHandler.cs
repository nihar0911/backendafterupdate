using MediatR;
using VendorManagementproj.Application.Contracts.Persistence;
using VendorManagementproj.Application.DTOs;

namespace VendorManagementproj.Application.Features.Products.Queries.GetAllProducts;

public class GetAllProductsQueryHandler : IRequestHandler<GetAllProductsQuery, GetAllProductsResponse>
{
    private readonly IProductRepository _productRepository;

    public GetAllProductsQueryHandler(IProductRepository productRepository)
    {
        _productRepository = productRepository;
    }

    public async Task<GetAllProductsResponse> Handle(GetAllProductsQuery request, CancellationToken cancellationToken)
    {
        var products = await _productRepository.GetAllAsync();

        var list = products.Select(p => new ProductDto
        {
            ProductID = p.ProductID,
            ProductName = p.ProductName,
            Category = p.Category,
            Unit = p.Unit,
            TaxRateID = p.TaxRateID,
            Status = p.Status
        }).ToList();

        return new GetAllProductsResponse
        {
            Products = list
        };

    }
}
