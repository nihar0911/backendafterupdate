using MediatR;

namespace VendorManagementprojApplication.Features.VendorRecommendations.Queries.GetRecommendationsForProduct;

public class GetRecommendationsForProductQuery : IRequest<GetRecommendationsForProductResponse>
{
    public int OutletID { get; set; }
    public int ProductID { get; set; }

    public GetRecommendationsForProductQuery() { }

    public GetRecommendationsForProductQuery(int outletID, int productID)
    {
        OutletID = outletID;
        ProductID = productID;
    }
}
