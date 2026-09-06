using VendorManagementprojApplication.DTOs;

namespace VendorManagementprojApplication.Features.Products.Queries.GetAllProducts;

public class GetAllProductsResponse
{
    public List<ProductDto> Products { get; set; } = new();
}
