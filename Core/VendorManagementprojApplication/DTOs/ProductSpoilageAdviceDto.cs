namespace VendorManagementprojApplication.DTOs;

public class ProductSpoilageAdviceDto
{
    public int ProductID { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string Unit { get; set; } = string.Empty;
    public int POItemID { get; set; }
    public decimal CurrentQuantity { get; set; }

    // Historical Delivery Statistics
    public int DeliveryCount { get; set; }
    public decimal TotalOrderedQuantity { get; set; }
    public decimal TotalReceivedQuantity { get; set; }
    public decimal TotalSpoiledQuantity { get; set; }

    // Spoilage Percentages
    public decimal? WeightedSpoilagePercentage { get; set; }
    public decimal? AverageSpoilagePercentage { get; set; }
    public decimal? RecentSpoilagePercentage { get; set; }
    public decimal? MinimumSpoilagePercentage { get; set; }
    public decimal? MaximumSpoilagePercentage { get; set; }

    // Trend & Risk
    public string Trend { get; set; } = "None";
    public string RiskLevel { get; set; } = "INSUFFICIENT DATA";

    // Dispatch & Advisory
    public int? PriorityRank { get; set; }
    public decimal? EstimatedSpoiledQuantity { get; set; }
    public string Why { get; set; } = string.Empty;
    public string RecommendedAction { get; set; } = string.Empty;
}
