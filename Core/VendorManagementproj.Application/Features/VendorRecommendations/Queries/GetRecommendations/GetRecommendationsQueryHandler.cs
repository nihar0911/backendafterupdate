using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using VendorManagementprojApplication.Contracts.Persistence;
using VendorManagementprojApplication.Contracts.Services;
using VendorManagementprojApplication.DTOs;

namespace VendorManagementprojApplication.Features.VendorRecommendations.Queries.GetRecommendations;

public class GetRecommendationsQueryHandler : IRequestHandler<GetRecommendationsQuery, GetRecommendationsResponse>
{
    private readonly IPurchaseRequestRepository _purchaseRequestRepository;
    private readonly IVendorProductRepository _vendorProductRepository;
    private readonly IContractRepository _contractRepository;
    private readonly IVendorRepository _vendorRepository;
    private readonly IVendorFeedbackRepository _vendorFeedbackRepository;
    private readonly IOutletRepository _outletRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly IVendorRecommendationSettingsRepository _settingsRepository;

    public GetRecommendationsQueryHandler(
        IPurchaseRequestRepository purchaseRequestRepository,
        IVendorProductRepository vendorProductRepository,
        IContractRepository contractRepository,
        IVendorRepository vendorRepository,
        IVendorFeedbackRepository vendorFeedbackRepository,
        IOutletRepository outletRepository,
        ICurrentUserService currentUserService,
        IVendorRecommendationSettingsRepository settingsRepository)
    {
        _purchaseRequestRepository = purchaseRequestRepository;
        _vendorProductRepository = vendorProductRepository;
        _contractRepository = contractRepository;
        _vendorRepository = vendorRepository;
        _vendorFeedbackRepository = vendorFeedbackRepository;
        _outletRepository = outletRepository;
        _currentUserService = currentUserService;
        _settingsRepository = settingsRepository;
    }

    public async Task<GetRecommendationsResponse> Handle(
        GetRecommendationsQuery request,
        CancellationToken cancellationToken)
    {
        if (request.PurchaseRequestID <= 0)
            throw new InvalidOperationException(
                "A valid purchase request is required.");

        var purchaseRequest =
            await _purchaseRequestRepository.GetByIdAsync(
                request.PurchaseRequestID);

        if (purchaseRequest == null)
            throw new InvalidOperationException(
                "Purchase request does not exist.");

        if ((_currentUserService.IsOutletManager || _currentUserService.IsPurchaseManager) && _currentUserService.OutletID.HasValue)
        {
            if (purchaseRequest.OutletID != _currentUserService.OutletID.Value)
            {
                throw new UnauthorizedAccessException("You are not authorized to view recommendations for a purchase request belonging to another outlet.");
            }
        }
        else if (_currentUserService.IsPurchaseManager && !_currentUserService.OutletID.HasValue)
        {
            throw new UnauthorizedAccessException("You are not assigned to an outlet.");
        }
        else if (_currentUserService.IsOrganizationManager && _currentUserService.OrganizationID.HasValue)
        {
            var orgOutlets = await _outletRepository.GetByOrganizationIdAsync(_currentUserService.OrganizationID.Value);
            var orgOutletIds = orgOutlets.Select(o => o.OutletID).ToHashSet();
            if (!orgOutletIds.Contains(purchaseRequest.OutletID))
            {
                throw new UnauthorizedAccessException("You are not authorized to view recommendations for a purchase request outside your organization.");
            }
        }

        var items =
            await _purchaseRequestRepository.GetItemsByRequestIdAsync(
                request.PurchaseRequestID);

        if (items.Count == 0)
            throw new InvalidOperationException(
                "Purchase request does not contain any items.");

        var settings = await _settingsRepository.GetSettingsAsync(cancellationToken);

        var recommendations =
            new List<VendorRecommendationDto>();

        var allVendors = (await _vendorRepository.GetAllAsync())
            .Where(v => string.Equals(v.Status, "Active", StringComparison.OrdinalIgnoreCase))
            .ToDictionary(v => v.VendorID, v => v.VendorName);

        foreach (var item in items)
        {
            // =========================================================================
            // STEP 1: Determine whether active contracts exist for Outlet + Product
            // =========================================================================
            var activeContracts = await _contractRepository.GetActiveContractsByProductAndOutletAsync(
                purchaseRequest.OutletID,
                item.ProductID);

            var activeVendorProducts = await _vendorProductRepository.GetEligibleVendorsForProductAsync(
                item.ProductID,
                purchaseRequest.OutletID);

            // =========================================================================
            // CASE A: Active Contracts Found (YES) -> Return contracted vendors
            // =========================================================================
            if (activeContracts.Count > 0)
            {
                var contractedRecommendations = new List<VendorRecommendationDto>();
                decimal minContractPrice = decimal.MaxValue;

                foreach (var contract in activeContracts)
                {
                    int vendorId = contract.VendorID ?? contract.VendorAllocations.FirstOrDefault()?.VendorID ?? 0;
                    var cp = contract.ContractProducts?.FirstOrDefault(p => p.ProductID == item.ProductID);
                    var vp = activeVendorProducts.FirstOrDefault(v => v.VendorID == vendorId);
                    decimal price = cp?.UnitPrice ?? vp?.UnitPrice ?? 0m;
                    if (price > 0 && price < minContractPrice)
                    {
                        minContractPrice = price;
                    }
                }
                if (minContractPrice == decimal.MaxValue) minContractPrice = 0m;

                foreach (var contract in activeContracts)
                {
                    int vendorId = contract.VendorID ?? contract.VendorAllocations.FirstOrDefault()?.VendorID ?? 0;
                    string vendorName = contract.Vendor?.VendorName 
                        ?? (allVendors.TryGetValue(vendorId, out var name) ? name : $"Vendor #{vendorId}");

                    var cp = contract.ContractProducts?.FirstOrDefault(p => p.ProductID == item.ProductID);
                    var vp = activeVendorProducts.FirstOrDefault(v => v.VendorID == vendorId);

                    decimal unitPrice = cp?.UnitPrice ?? vp?.UnitPrice ?? 0m;
                    int deliveryDays = vp?.EstimatedDeliveryDays ?? 1;

                    bool hasContractProducts = contract.ContractProducts != null && contract.ContractProducts.Count > 0;
                    decimal contractQty = cp?.ContractQuantity ?? (hasContractProducts ? 0m : contract.TotalQuantity);
                    decimal purchasedQty = cp?.PurchasedQuantity ?? (hasContractProducts ? 0m : contract.UsedQuantity);
                    decimal remainingQty = Math.Max(0m, contractQty - purchasedQty);
                    decimal contractTotalQty = hasContractProducts
                        ? contract.ContractProducts!.Sum(p => p.ContractQuantity)
                        : contract.TotalQuantity;

                    var feedbacks = await _vendorFeedbackRepository.GetByVendorIdAsync(vendorId);
                    int feedbackCount = feedbacks?.Count ?? 0;

                    decimal avgRating = feedbackCount > 0
                        ? Math.Round(feedbacks!.Average(f => f.Rating), 1)
                        : 0m;
                    decimal avgQuality = feedbackCount > 0
                        ? Math.Round(feedbacks!.Average(f => f.ProductQualityRating), 1)
                        : 0m;
                    decimal avgDelivery = feedbackCount > 0
                        ? Math.Round(feedbacks!.Average(f => f.DeliveryRating), 1)
                        : 0m;

                    decimal qualityScore = feedbackCount > 0 ? (avgQuality / 5.0m) * 100m : settings.NeutralScoreForNewVendors;
                    decimal deliveryScore = feedbackCount > 0 ? (avgDelivery / 5.0m) * 100m : settings.NeutralScoreForNewVendors;
                    decimal priceScore = (unitPrice > 0 && minContractPrice > 0)
                        ? Math.Round((minContractPrice / unitPrice) * 100m, 1)
                        : settings.NeutralScoreForNewVendors;
                    decimal reliabilityBonus = Math.Min(settings.ReliabilityWeight, feedbackCount * settings.ReliabilityPointsPerReview);

                    decimal overallCompositeScore = Math.Round(
                        (qualityScore * (settings.QualityWeight / 100m)) +
                        (deliveryScore * (settings.DeliveryWeight / 100m)) +
                        (priceScore * (settings.PriceWeight / 100m)) +
                        reliabilityBonus,
                        1
                    );

                    contractedRecommendations.Add(new VendorRecommendationDto
                    {
                        VendorID = vendorId,
                        VendorName = vendorName,
                        ProductID = item.ProductID,
                        ProductName = item.Product?.ProductName ?? cp?.Product?.ProductName ?? $"Product #{item.ProductID}",
                        UnitPrice = unitPrice,
                        EstimatedDeliveryDays = deliveryDays,
                        OverallScore = overallCompositeScore,
                        AverageRating = avgRating,
                        AverageQualityRating = avgQuality,
                        AverageDeliveryRating = avgDelivery,
                        TotalFeedbackCount = feedbackCount,
                        DeliveryCompletionPercentage = deliveryScore,
                        AverageSpoilagePercentage = 0m,
                        HasActiveContract = true,
                        ContractID = contract.ContractID,
                        ContractQuantity = contractQty,
                        PurchasedQuantity = purchasedQty,
                        AllocatedQuantity = contractQty,
                        UsedQuantity = purchasedQty,
                        RemainingQuantity = remainingQty,
                        ContractTotalQuantity = contractTotalQty,
                        ContractStartDate = contract.StartDate,
                        ContractEndDate = contract.EndDate,
                        ContractStatus = contract.Status,
                        Recommendation = string.Empty,
                        SmartBadge = string.Empty
                    });
                }

                var rankedContractVendors = contractedRecommendations
                    .OrderByDescending(r => r.OverallScore)
                    .ThenBy(r => r.UnitPrice)
                    .ThenBy(r => r.EstimatedDeliveryDays)
                    .ThenBy(r => r.VendorID)
                    .ToList();

                for (int i = 0; i < rankedContractVendors.Count; i++)
                {
                    var rec = rankedContractVendors[i];
                    rec.Rank = i + 1;

                    if (i == 0)
                    {
                        rec.SmartBadge = "Top Recommended";
                        rec.Recommendation = rec.TotalFeedbackCount > 0
                            ? $"Top Recommended: Active contract with {rec.VendorName} (Rating: {rec.AverageRating:0.0}, Unit Price: Rs. {rec.UnitPrice:N2})."
                            : $"Top Recommended: Priority active contract with {rec.VendorName} at Rs. {rec.UnitPrice:N2}.";
                    }
                    else
                    {
                        rec.SmartBadge = "Active Contract";
                        rec.Recommendation = "Active Contract";
                    }
                }

                recommendations.AddRange(rankedContractVendors);
                continue;
            }

            // =========================================================================
            // CASE B: No Contract Fallback (NO) -> Existing recommendation system
            // =========================================================================
            if (!activeVendorProducts.Any())
                continue;

            decimal minPrice = activeVendorProducts.Min(vp => vp.UnitPrice);

            var productRecommendations = new List<VendorRecommendationDto>();

            foreach (var vendorProduct in activeVendorProducts)
            {
                string vendorName = allVendors.TryGetValue(vendorProduct.VendorID, out var name)
                    ? name
                    : $"Vendor #{vendorProduct.VendorID}";

                // Retrieve historical reviews and feedback for this vendor
                var feedbacks = await _vendorFeedbackRepository.GetByVendorIdAsync(vendorProduct.VendorID);
                int feedbackCount = feedbacks?.Count ?? 0;

                decimal avgRating = feedbackCount > 0
                    ? Math.Round(feedbacks!.Average(f => f.Rating), 1)
                    : 0m;
                decimal avgQuality = feedbackCount > 0
                    ? Math.Round(feedbacks!.Average(f => f.ProductQualityRating), 1)
                    : 0m;
                decimal avgDelivery = feedbackCount > 0
                    ? Math.Round(feedbacks!.Average(f => f.DeliveryRating), 1)
                    : 0m;

                // Composite Multi-Factor Score (0 - 100):
                decimal qualityScore = feedbackCount > 0 ? (avgQuality / 5.0m) * 100m : settings.NeutralScoreForNewVendors;
                decimal deliveryScore = feedbackCount > 0 ? (avgDelivery / 5.0m) * 100m : settings.NeutralScoreForNewVendors;
                decimal priceScore = (vendorProduct.UnitPrice > 0 && minPrice > 0)
                    ? Math.Round((minPrice / vendorProduct.UnitPrice) * 100m, 1)
                    : settings.NeutralScoreForNewVendors;
                decimal reliabilityBonus = Math.Min(settings.ReliabilityWeight, feedbackCount * settings.ReliabilityPointsPerReview);

                decimal overallCompositeScore = Math.Round(
                    (qualityScore * (settings.QualityWeight / 100m)) +
                    (deliveryScore * (settings.DeliveryWeight / 100m)) +
                    (priceScore * (settings.PriceWeight / 100m)) +
                    reliabilityBonus,
                    1
                );

                productRecommendations.Add(
                    new VendorRecommendationDto
                    {
                        VendorID = vendorProduct.VendorID,
                        VendorName = vendorName,
                        ProductID = item.ProductID,
                        ProductName = item.Product?.ProductName ?? $"Product #{item.ProductID}",
                        UnitPrice = vendorProduct.UnitPrice,
                        EstimatedDeliveryDays = vendorProduct.EstimatedDeliveryDays,
                        OverallScore = overallCompositeScore,
                        AverageRating = avgRating,
                        AverageQualityRating = avgQuality,
                        AverageDeliveryRating = avgDelivery,
                        TotalFeedbackCount = feedbackCount,
                        DeliveryCompletionPercentage = deliveryScore,
                        AverageSpoilagePercentage = 0m,
                        Recommendation = string.Empty,
                        SmartBadge = string.Empty,
                        HasActiveContract = false,
                        ContractID = null
                    });
            }

            // AUTHORITATIVE RECOMMENDATION RANKING:
            var rankedQuery = productRecommendations.AsEnumerable();
            rankedQuery = rankedQuery.OrderByDescending(r => r.OverallScore)
                .ThenBy(r => r.UnitPrice)
                .ThenBy(r => r.EstimatedDeliveryDays)
                .ThenBy(r => r.VendorID);
            productRecommendations = rankedQuery.ToList();

            for (int i = 0; i < productRecommendations.Count; i++)
            {
                var rec = productRecommendations[i];
                rec.Rank = i + 1;

                // Determine Smart Badges
                if (i == 0)
                {
                    rec.SmartBadge = "Top Recommended";
                }
                else if (rec.AverageQualityRating >= settings.BestQualityThreshold)
                {
                    rec.SmartBadge = "Best Quality";
                }
                else if (rec.UnitPrice == minPrice)
                {
                    rec.SmartBadge = "Best Price";
                }
                else if (rec.AverageDeliveryRating >= settings.FastestDeliveryThreshold)
                {
                    rec.SmartBadge = "Fastest Delivery";
                }

                // AI / Smart Recommendation Rationale
                if (rec.UnitPrice == minPrice)
                {
                    rec.Recommendation = rec.TotalFeedbackCount > 0
                        ? $"Alternative — Best Price: Lowest unit price (Rs. {rec.UnitPrice:N2}) with {rec.AverageRating:0.0} customer rating."
                        : "Alternative — Best Price";
                }
                else
                {
                    rec.Recommendation = rec.TotalFeedbackCount > 0 && rec.AverageQualityRating >= settings.HighQualityRationaleThreshold
                        ? $"Alternative: High quality rating ({rec.AverageQualityRating:0.0}/5) across {rec.TotalFeedbackCount} past deliveries."
                        : "Alternative";
                }
            }

            recommendations.AddRange(productRecommendations);
        }

        return new GetRecommendationsResponse
        {
            PurchaseRequestID = request.PurchaseRequestID,
            Recommendations = recommendations
        };
    }
}
