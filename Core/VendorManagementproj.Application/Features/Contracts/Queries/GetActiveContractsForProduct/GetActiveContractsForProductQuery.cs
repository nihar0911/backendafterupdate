using MediatR;

namespace VendorManagementprojApplication.Features.Contracts.Queries.GetActiveContractsForProduct;

public class GetActiveContractsForProductQuery : IRequest<GetActiveContractsForProductResponse>
{
    public int OutletID { get; set; }
    public int ProductID { get; set; }

    public GetActiveContractsForProductQuery(int outletID, int productID)
    {
        OutletID = outletID;
        ProductID = productID;
    }
}
