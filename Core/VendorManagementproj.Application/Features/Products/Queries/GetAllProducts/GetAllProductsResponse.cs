using VendorManagementproj.Application.DTOs;

namespace VendorManagementproj.Application.Features.Products.Queries.GetAllProducts;

public class GetAllProductsResponse
{
    public List<ProductDto> Products { get; set; } = new();
}
