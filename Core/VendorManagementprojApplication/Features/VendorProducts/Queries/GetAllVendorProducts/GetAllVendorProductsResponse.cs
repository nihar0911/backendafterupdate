using VendorManagementprojDomain.Entities;

namespace VendorManagementprojApplication.Features.VendorProducts.Queries.GetAllVendorProducts;

public class GetAllVendorProductsResponse
{
    public List<VendorProduct> VendorProducts { get; set; } = new();
}
