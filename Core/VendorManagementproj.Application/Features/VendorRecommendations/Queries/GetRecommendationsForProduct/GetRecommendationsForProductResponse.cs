using System.Collections.Generic;
using VendorManagementprojApplication.DTOs;

namespace VendorManagementprojApplication.Features.VendorRecommendations.Queries.GetRecommendationsForProduct;

public class GetRecommendationsForProductResponse
{
    public int OutletID { get; set; }
    public int ProductID { get; set; }
    public bool HasActiveContracts { get; set; }
    public List<VendorRecommendationDto> Recommendations { get; set; } = new();
}
