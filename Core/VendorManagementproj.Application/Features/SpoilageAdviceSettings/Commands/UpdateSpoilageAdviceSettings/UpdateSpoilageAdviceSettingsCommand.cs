using MediatR;

namespace VendorManagementprojApplication.Features.SpoilageAdviceSettings.Commands.UpdateSpoilageAdviceSettings;

public class UpdateSpoilageAdviceSettingsCommand : IRequest<UpdateSpoilageAdviceSettingsResponse>
{
    public int RecentDeliveriesCount { get; set; }

    public decimal TrendTolerancePercentage { get; set; }

    public decimal HighWeightedSpoilageThreshold { get; set; }

    public decimal HighRecentSpoilageThreshold { get; set; }

    public decimal HighMaximumSpoilageThreshold { get; set; }

    public decimal MediumWeightedSpoilageThreshold { get; set; }

    public decimal LowWeightedSpoilageThreshold { get; set; }
}
