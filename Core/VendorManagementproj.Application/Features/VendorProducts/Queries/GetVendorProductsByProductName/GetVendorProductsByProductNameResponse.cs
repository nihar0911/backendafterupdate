using System.Collections.Generic;
using VendorManagementprojApplication.DTOs;

namespace VendorManagementprojApplication.Features.VendorProducts.Queries.GetVendorProductsByProductName;

public class GetVendorProductsByProductNameResponse
{
    public List<VendorProductDto> VendorProducts { get; set; } = new();
}
