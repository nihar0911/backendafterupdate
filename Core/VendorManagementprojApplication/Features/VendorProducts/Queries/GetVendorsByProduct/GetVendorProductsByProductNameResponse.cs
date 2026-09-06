using System.Collections.Generic;
using VendorManagementprojDomain.Entities;

namespace VendorManagementprojApplication.Features.VendorProducts.Queries.GetVendorProductsByProductName;

public class GetVendorProductsByProductNameResponse
{
    public List<VendorProduct> VendorProducts { get; set; } = new();
}