using System;

namespace VendorManagementprojDomain.Entities;

public class SpoilageAdviceSettings
{
    public int Id { get; set; }

    public int RecentDeliveriesCount { get; set; } = 5;

    public decimal TrendTolerancePercentage { get; set; } = 1.0m;

    public decimal HighWeightedSpoilageThreshold { get; set; } = 5.0m;

    public decimal HighRecentSpoilageThreshold { get; set; } = 6.0m;

    public decimal HighMaximumSpoilageThreshold { get; set; } = 10.0m;

    public decimal MediumWeightedSpoilageThreshold { get; set; } = 2.0m;

    public decimal LowWeightedSpoilageThreshold { get; set; } = 2.0m;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
