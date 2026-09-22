using System.Collections.Generic;
using VendorManagementproj.Application.DTOs;

namespace VendorManagementproj.Application.Features.VendorRecommendations.Queries.GetRecommendationsForProduct;

public class GetRecommendationsForProductResponse
{
    public int OutletID { get; set; }
    public int ProductID { get; set; }
    public bool HasActiveContracts { get; set; }
    public List<VendorRecommendationDto> Recommendations { get; set; } = new();
}
