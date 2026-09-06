using MediatR;

namespace VendorManagementprojApplication.Features.VendorRecommendations.Queries.GetRecommendations;

public class Query : IRequest<Response>
{
    public int PurchaseRequestID { get; set; }
}