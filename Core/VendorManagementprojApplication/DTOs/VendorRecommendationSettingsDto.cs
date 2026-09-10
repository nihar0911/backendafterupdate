using System;

namespace VendorManagementprojApplication.DTOs;

public class VendorRecommendationSettingsDto
{
    public int Id { get; set; }

    public decimal QualityWeight { get; set; }

    public decimal DeliveryWeight { get; set; }

    public decimal PriceWeight { get; set; }

    public decimal ReliabilityWeight { get; set; }

    public decimal ReliabilityPointsPerReview { get; set; }

    public decimal NeutralScoreForNewVendors { get; set; }

    public decimal BestQualityThreshold { get; set; }

    public decimal FastestDeliveryThreshold { get; set; }

    public decimal HighQualityRationaleThreshold { get; set; }

    public bool PrioritizeActiveContracts { get; set; }

    public DateTime UpdatedAt { get; set; }
}
