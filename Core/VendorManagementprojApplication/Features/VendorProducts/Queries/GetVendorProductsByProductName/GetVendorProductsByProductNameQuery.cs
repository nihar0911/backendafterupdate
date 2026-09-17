using MediatR;

namespace VendorManagementprojApplication.Features.VendorProducts.Queries.GetVendorProductsByProductName;

public class GetVendorProductsByProductNameQuery : IRequest<GetVendorProductsByProductNameResponse>
{
    public string ProductName { get; set; }

    public GetVendorProductsByProductNameQuery(string productName)
    {
        ProductName = productName;
    }
}
