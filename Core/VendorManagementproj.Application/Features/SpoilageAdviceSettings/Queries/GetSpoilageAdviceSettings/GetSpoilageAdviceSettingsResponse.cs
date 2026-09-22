using VendorManagementproj.Application.DTOs;

namespace VendorManagementproj.Application.Features.SpoilageAdviceSettings.Queries.GetSpoilageAdviceSettings;

public class GetSpoilageAdviceSettingsResponse
{
    public SpoilageAdviceSettingsDto Settings { get; set; } = null!;
}
