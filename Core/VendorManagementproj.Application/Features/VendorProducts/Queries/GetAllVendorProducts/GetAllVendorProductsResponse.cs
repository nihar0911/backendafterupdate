using System.Collections.Generic;
using VendorManagementproj.Application.DTOs;

namespace VendorManagementproj.Application.Features.VendorProducts.Queries.GetAllVendorProducts;

public class GetAllVendorProductsResponse
{
    public List<VendorProductDto> VendorProducts { get; set; } = new();
}
