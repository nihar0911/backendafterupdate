using MediatR;
using VendorManagementprojApplication.Contracts.Persistence;
using VendorManagementprojApplication.DTOs;
using VendorManagementprojDomain.Entities;

namespace VendorManagementprojApplication.Features.Products.Commands.CreateProduct;

public class CreateProductCommandHandler : IRequestHandler<CreateProductCommand, CreateProductResponse>
{
    private readonly IProductRepository _productRepository;
    private readonly ITaxRateRepository _taxRateRepository;

    public CreateProductCommandHandler(
        IProductRepository productRepository,
        ITaxRateRepository taxRateRepository)
    {
        _productRepository = productRepository;
        _taxRateRepository = taxRateRepository;
    }

    public async Task<CreateProductResponse> Handle(CreateProductCommand request, CancellationToken cancellationToken)
    {
        var taxRate = await _taxRateRepository.GetByIdAsync(request.TaxRateID);
        if (taxRate == null)
        {
            throw new InvalidOperationException($"TaxRateID {request.TaxRateID} does not exist.");
        }

        if (!string.Equals(taxRate.Status, "Active", StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException($"TaxRateID {request.TaxRateID} is not active.");
        }

        var product = new Product
        {
            ProductName = request.ProductName,
            Category = request.Category,
            Unit = request.Unit,
            TaxRateID = request.TaxRateID,
            Status = request.Status
        };

        var createdProduct = await _productRepository.AddAsync(product);

        var dto = new ProductDto
        {
            ProductID = createdProduct.ProductID,
            ProductName = createdProduct.ProductName,
            Category = createdProduct.Category,
            Unit = createdProduct.Unit,
            TaxRateID = createdProduct.TaxRateID ,
            Status = createdProduct.Status
        };

        return new CreateProductResponse
        {
            Product = dto
        };
    }
}
