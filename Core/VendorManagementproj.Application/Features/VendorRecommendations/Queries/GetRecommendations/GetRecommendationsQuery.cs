using MediatR;

namespace VendorManagementprojApplication.Features.VendorRecommendations.Queries.GetRecommendations;

public class GetRecommendationsQuery : IRequest<GetRecommendationsResponse>
{
    public int PurchaseRequestID { get; set; }
}
