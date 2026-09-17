using System.Collections.Generic;
using VendorManagementprojApplication.DTOs;

namespace VendorManagementprojApplication.Features.VendorProducts.Queries.GetAllVendorProducts;

public class GetAllVendorProductsResponse
{
    public List<VendorProductDto> VendorProducts { get; set; } = new();
}
