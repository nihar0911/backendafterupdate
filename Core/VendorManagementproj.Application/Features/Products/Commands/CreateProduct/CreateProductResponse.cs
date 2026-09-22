using VendorManagementproj.Application.DTOs;

namespace VendorManagementproj.Application.Features.Products.Commands.CreateProduct;

public class CreateProductResponse
{
    public ProductDto Product { get; set; } = null!;
}
