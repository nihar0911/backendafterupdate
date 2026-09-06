using MediatR;

namespace VendorManagementprojApplication.Features.VendorProducts.Queries.GetVendorsByProduct;

public class GetVendorsByProductQuery : IRequest<GetVendorsByProductResponse>
{
    public string ProductName { get; set; } = string.Empty;
    public int OutletID { get; set; }
}