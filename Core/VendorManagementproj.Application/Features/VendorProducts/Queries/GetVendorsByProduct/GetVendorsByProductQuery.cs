using MediatR;

namespace VendorManagementproj.Application.Features.VendorProducts.Queries.GetVendorsByProduct;

public class GetVendorsByProductQuery : IRequest<GetVendorsByProductResponse>
{
    public string ProductName { get; set; } = string.Empty;
    public int OutletID { get; set; }
}