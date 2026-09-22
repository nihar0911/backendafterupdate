using System.Collections.Generic;
using VendorManagementprojApplication.DTOs;

namespace VendorManagementprojApplication.Features.VendorRecommendations.Queries.GetRecommendations;

public class GetRecommendationsResponse
{
    public int PurchaseRequestID { get; set; }

    public List<VendorRecommendationDto> Recommendations { get; set; } = new();
}
