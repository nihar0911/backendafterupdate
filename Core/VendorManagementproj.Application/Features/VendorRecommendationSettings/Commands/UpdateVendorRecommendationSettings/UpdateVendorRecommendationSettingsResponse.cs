using VendorManagementprojApplication.DTOs;

namespace VendorManagementprojApplication.Features.VendorRecommendationSettings.Commands.UpdateVendorRecommendationSettings;

public class UpdateVendorRecommendationSettingsResponse
{
    public string Message { get; set; } = string.Empty;

    public VendorRecommendationSettingsDto Settings { get; set; } = new();
}
