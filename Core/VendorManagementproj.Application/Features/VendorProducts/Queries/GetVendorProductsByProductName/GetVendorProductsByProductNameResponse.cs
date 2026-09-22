using System.Collections.Generic;
using VendorManagementproj.Application.DTOs;

namespace VendorManagementproj.Application.Features.VendorProducts.Queries.GetVendorProductsByProductName;

public class GetVendorProductsByProductNameResponse
{
    public List<VendorProductDto> VendorProducts { get; set; } = new();
}
