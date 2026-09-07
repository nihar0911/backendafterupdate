namespace VendorManagementprojApplication.DTOs;

public class VendorAnalysisDto
{
    public int VendorID { get; set; }

    public string VendorName { get; set; } = string.Empty;

    public decimal AverageRating { get; set; }

    public decimal RatingScore { get; set; }

    public decimal DeliveryScore { get; set; }

    public decimal ContractScore { get; set; }

    public decimal OverallScore { get; set; }

    public int TotalFeedbackCount { get; set; }

    public decimal DeliveryCompletionPercentage { get; set; }

    public decimal AverageSpoilagePercentage { get; set; }

    public decimal ContractFulfillmentPercentage { get; set; }

    public string Recommendation { get; set; } = string.Empty;
}