using System.Collections.Generic;
using VendorManagementproj.Application.DTOs;

namespace VendorManagementproj.Application.Features.VendorRecommendations.Queries.GetRecommendations;

public class GetRecommendationsResponse
{
    public int PurchaseRequestID { get; set; }

    public List<VendorRecommendationDto> Recommendations { get; set; } = new();
}
