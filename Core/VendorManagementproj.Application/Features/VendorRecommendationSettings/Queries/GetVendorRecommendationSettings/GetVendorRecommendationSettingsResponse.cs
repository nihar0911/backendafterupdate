using VendorManagementproj.Application.DTOs;

namespace VendorManagementproj.Application.Features.VendorRecommendationSettings.Queries.GetVendorRecommendationSettings;

public class GetVendorRecommendationSettingsResponse
{
    public VendorRecommendationSettingsDto Settings { get; set; } = new();
}
