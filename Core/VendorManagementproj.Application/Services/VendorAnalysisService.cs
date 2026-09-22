using VendorManagementprojApplication.Contracts.Persistence;
using VendorManagementprojApplication.Contracts.Services;
using VendorManagementprojApplication.DTOs;

namespace VendorManagementprojApplication.Services;

public class VendorAnalysisService : IVendorAnalysisService
{
    private readonly IVendorRepository _vendorRepository;
    private readonly IVendorFeedbackRepository _feedbackRepository;
    private readonly IDeliveryRecordRepository _deliveryRecordRepository;
    private readonly IContractRepository _contractRepository;

    public VendorAnalysisService(
        IVendorRepository vendorRepository,
        IVendorFeedbackRepository feedbackRepository,
        IDeliveryRecordRepository deliveryRecordRepository,
        IContractRepository contractRepository)
    {
        _vendorRepository = vendorRepository;
        _feedbackRepository = feedbackRepository;
        _deliveryRecordRepository = deliveryRecordRepository;
        _contractRepository = contractRepository;
    }

    public async Task<VendorAnalysisDto> AnalyzeVendorAsync(
        int vendorID,
        int productID,
        int outletID)
    {
        var vendor =
            await _vendorRepository.GetByIdAsync(vendorID);

        if (vendor == null)
            throw new InvalidOperationException(
                "Vendor does not exist.");

        var allFeedback =
            await _feedbackRepository.GetAllAsync();

        var vendorFeedback = allFeedback
            .Where(f => f.VendorID == vendorID)
            .ToList();

        decimal averageRating = 0;

        if (vendorFeedback.Count > 0)
            averageRating =
                vendorFeedback.Average(f => f.Rating);

        decimal ratingScore =
            (averageRating / 5m) * 40m;

        var vendorDeliveries =
            (await _deliveryRecordRepository
                .GetByVendorProductOutletAsync(
                    vendorID,
                    productID,
                    outletID))
            .Where(d =>
                string.Equals(
                    d.Status,
                    "Confirmed",
                    StringComparison.OrdinalIgnoreCase))
            .ToList();
        decimal deliveryCompletionPercentage = 0;
        decimal averageSpoilagePercentage = 0;

        if (vendorDeliveries.Count > 0)
        {
            decimal totalOrdered =
                vendorDeliveries.Sum(
                    d => d.OrderedQuantity);

            decimal totalReceived =
                vendorDeliveries.Sum(
                    d => d.ReceivedQuantity);

            if (totalOrdered > 0)
            {
                deliveryCompletionPercentage =
                    (totalReceived / totalOrdered) * 100m;
            }

            averageSpoilagePercentage =
                vendorDeliveries.Average(
                    d => d.SpoilagePercentage);
        }

        decimal quantityScore =
            deliveryCompletionPercentage;

        decimal spoilageScore =
            Math.Max(
                0,
                100m - averageSpoilagePercentage);

        decimal deliveryPerformance =
            (quantityScore * 0.80m) +
            (spoilageScore * 0.20m);

        decimal deliveryScore =
            (deliveryPerformance / 100m) * 40m;

        deliveryScore =
            Math.Min(
                40m,
                Math.Max(
                    0m,
                    deliveryScore));

        var vendorAllocations =
     await _contractRepository
         .GetVendorAllocationsForAnalysisAsync(
             vendorID,
             productID,
             outletID);

        decimal contractFulfillmentPercentage = 0;

        if (vendorAllocations.Count > 0)
        {
            decimal allocatedQuantity =
                vendorAllocations.Sum(
                    a => a.AllocatedQuantity);

            decimal usedQuantity =
                vendorAllocations.Sum(
                    a => a.UsedQuantity);

            if (allocatedQuantity > 0)
            {
                contractFulfillmentPercentage =
                    (usedQuantity / allocatedQuantity) * 100m;
            }
        }

        decimal contractScore =
            Math.Min(
                20m,
                Math.Max(
                    0m,
                    (contractFulfillmentPercentage / 100m) * 20m));

        decimal overallScore =
            ratingScore +
            deliveryScore +
            contractScore;

        string recommendation;

        if (overallScore >= 85)
            recommendation = "Highly Recommended";
        else if (overallScore >= 70)
            recommendation = "Recommended";
        else if (overallScore >= 55)
            recommendation = "Consider";
        else
            recommendation = "Low Priority";

        return new VendorAnalysisDto
        {
            VendorID = vendorID,
            VendorName = vendor.VendorName,
            AverageRating =
                Math.Round(averageRating, 2),
            RatingScore =
                Math.Round(ratingScore, 2),
            DeliveryScore =
                Math.Round(deliveryScore, 2),
            ContractScore =
                Math.Round(contractScore, 2),
            OverallScore =
                Math.Round(overallScore, 2),
            TotalFeedbackCount =
                vendorFeedback.Count,
            DeliveryCompletionPercentage =
                Math.Round(
                    deliveryCompletionPercentage,
                    2),
            AverageSpoilagePercentage =
                Math.Round(
                    averageSpoilagePercentage,
                    2),
            ContractFulfillmentPercentage =
                Math.Round(
                    contractFulfillmentPercentage,
                    2),
            Recommendation =
                recommendation
        };
    }
}