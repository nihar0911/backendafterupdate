using VendorManagementprojApplication.DTOs;

namespace VendorManagementprojApplication.Features.VendorRecommendationSettings.Queries.GetVendorRecommendationSettings;

public class GetVendorRecommendationSettingsResponse
{
    public VendorRecommendationSettingsDto Settings { get; set; } = new();
}
