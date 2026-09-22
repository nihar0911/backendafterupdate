using VendorManagementproj.Application.DTOs;

namespace VendorManagementproj.Application.Features.VendorProducts.Queries.GetVendorsByProduct;

public class GetVendorsByProductResponse
{
    public List<VendorProductSearchDto> Vendors { get; set; } = new();
}