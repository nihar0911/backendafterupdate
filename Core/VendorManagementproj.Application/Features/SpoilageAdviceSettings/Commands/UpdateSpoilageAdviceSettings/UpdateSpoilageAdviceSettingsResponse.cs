using VendorManagementproj.Application.DTOs;

namespace VendorManagementproj.Application.Features.SpoilageAdviceSettings.Commands.UpdateSpoilageAdviceSettings;

public class UpdateSpoilageAdviceSettingsResponse
{
    public SpoilageAdviceSettingsDto Settings { get; set; } = null!;

    public string Message { get; set; } = "Spoilage advice settings saved successfully.";
}
