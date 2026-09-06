using VendorManagementprojApplication.DTOs;

namespace VendorManagementprojApplication.Features.VendorProducts.Queries.GetVendorsByProduct;

public class GetVendorsByProductResponse
{
    public List<VendorProductSearchDto> Vendors { get; set; } = new();
}