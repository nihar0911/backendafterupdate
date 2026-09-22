using System;

namespace VendorManagementprojApplication.DTOs;

public class SpoilageAdviceSettingsDto
{
    public int Id { get; set; }

    public int RecentDeliveriesCount { get; set; }

    public decimal TrendTolerancePercentage { get; set; }

    public decimal HighWeightedSpoilageThreshold { get; set; }

    public decimal HighRecentSpoilageThreshold { get; set; }

    public decimal HighMaximumSpoilageThreshold { get; set; }

    public decimal MediumWeightedSpoilageThreshold { get; set; }

    public decimal LowWeightedSpoilageThreshold { get; set; }

    public DateTime? UpdatedAt { get; set; }
}
