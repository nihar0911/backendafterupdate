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

public class Handler : IRequestHandler<Query, Response>
{
    private readonly IPurchaseRequestRepository _purchaseRequestRepository;
    private readonly IVendorProductRepository _vendorProductRepository;
    private readonly IContractRepository _contractRepository;
    private readonly IVendorRepository _vendorRepository;
    private readonly IVendorFeedbackRepository _vendorFeedbackRepository;
    private readonly IOutletRepository _outletRepository;
    private readonly ICurrentUserService _currentUserService;

    public Handler(
        IPurchaseRequestRepository purchaseRequestRepository,
        IVendorProductRepository vendorProductRepository,
        IContractRepository contractRepository,
        IVendorRepository vendorRepository,
        IVendorFeedbackRepository vendorFeedbackRepository,
        IOutletRepository outletRepository,
        ICurrentUserService currentUserService)
    {
        _purchaseRequestRepository = purchaseRequestRepository;
        _vendorProductRepository = vendorProductRepository;
        _contractRepository = contractRepository;
        _vendorRepository = vendorRepository;
        _vendorFeedbackRepository = vendorFeedbackRepository;
        _outletRepository = outletRepository;
        _currentUserService = currentUserService;
    }

    public async Task<Response> Handle(
        Query request,
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

        var recommendations =
            new List<VendorRecommendationDto>();

        var allVendors = (await _vendorRepository.GetAllAsync())
            .Where(v => string.Equals(v.Status, "Active", StringComparison.OrdinalIgnoreCase))
            .ToDictionary(v => v.VendorID, v => v.VendorName);

        foreach (var item in items)
        {
            var activeVendorProducts =
                await _vendorProductRepository.GetEligibleVendorsForProductAsync(
                    item.ProductID,
                    purchaseRequest.OutletID);

            if (!activeVendorProducts.Any())
                continue;

            decimal minPrice = activeVendorProducts.Min(vp => vp.UnitPrice);

            var productRecommendations = new List<VendorRecommendationDto>();

            foreach (var vendorProduct in activeVendorProducts)
            {
                string vendorName = allVendors.TryGetValue(vendorProduct.VendorID, out var name)
                    ? name
                    : $"Vendor #{vendorProduct.VendorID}";

                var allocations =
                    await _contractRepository.GetVendorAllocationsAsync(
                        vendorProduct.VendorID,
                        item.ProductID,
                        purchaseRequest.OutletID);

                // Group 1 qualification: Active contract with remaining capacity > 0
                var usableAllocations = allocations
                    .Where(a => (a.AllocatedQuantity - a.UsedQuantity) > 0)
                    .ToList();

                bool hasActiveUsableContract = usableAllocations.Count > 0;
                var primaryAllocation = usableAllocations.FirstOrDefault() ?? allocations.FirstOrDefault();

                decimal allocatedQty = 0m;
                decimal usedQty = 0m;
                decimal remainingQty = 0m;
                decimal allocationPercentage = 0m;
                int? contractId = null;

                if (primaryAllocation != null)
                {
                    contractId = primaryAllocation.ContractID;
                    allocationPercentage = primaryAllocation.AllocationPercentage;
                    allocatedQty = primaryAllocation.AllocatedQuantity;
                    usedQty = primaryAllocation.UsedQuantity;
                    remainingQty = Math.Max(0m, primaryAllocation.AllocatedQuantity - primaryAllocation.UsedQuantity);
                }

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
                // Quality (35%): 5.0 -> 100 pts. If no reviews, neutral 70.
                decimal qualityScore = feedbackCount > 0 ? (avgQuality / 5.0m) * 100m : 70m;

                // Delivery (25%): 5.0 -> 100 pts. If no reviews, neutral 70.
                decimal deliveryScore = feedbackCount > 0 ? (avgDelivery / 5.0m) * 100m : 70m;

                // Price Competitiveness (25%): lowest price = 100 pts
                decimal priceScore = (vendorProduct.UnitPrice > 0 && minPrice > 0)
                    ? Math.Round((minPrice / vendorProduct.UnitPrice) * 100m, 1)
                    : 70m;

                // Volume & Reliability Bonus (15%): established reviews give up to 15 pts
                decimal reliabilityBonus = Math.Min(15m, feedbackCount * 3m);

                decimal overallCompositeScore = Math.Round(
                    (qualityScore * 0.35m) +
                    (deliveryScore * 0.25m) +
                    (priceScore * 0.25m) +
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
                        HasActiveContract = hasActiveUsableContract,
                        ContractID = contractId,
                        AllocationPercentage = allocationPercentage,
                        AllocatedQuantity = allocatedQty,
                        UsedQuantity = usedQty,
                        RemainingQuantity = remainingQty
                    });
            }

            // AUTHORITATIVE RECOMMENDATION RANKING:
            // 1. Group 1 (Active Contract with RemainingQuantity > 0) strictly above Group 2
            // 2. Highest OverallCompositeScore (Quality + Delivery + Price + Volume)
            // 3. Lowest Unit Price
            // 4. Lowest EstimatedDeliveryDays
            // 5. VendorID as deterministic tie-breaker
            productRecommendations = productRecommendations
                .OrderByDescending(r => r.HasActiveContract && r.RemainingQuantity > 0)
                .ThenByDescending(r => r.OverallScore)
                .ThenBy(r => r.UnitPrice)
                .ThenBy(r => r.EstimatedDeliveryDays)
                .ThenBy(r => r.VendorID)
                .ToList();

            for (int i = 0; i < productRecommendations.Count; i++)
            {
                var rec = productRecommendations[i];
                rec.Rank = i + 1;

                // Determine Smart Badges
                if (i == 0)
                {
                    rec.SmartBadge = "Top Recommended";
                }
                else if (rec.AverageQualityRating >= 4.5m)
                {
                    rec.SmartBadge = "Best Quality";
                }
                else if (rec.UnitPrice == minPrice)
                {
                    rec.SmartBadge = "Best Price";
                }
                else if (rec.AverageDeliveryRating >= 4.5m)
                {
                    rec.SmartBadge = "Fastest Delivery";
                }

                // AI / Smart Recommendation Rationale
                if (rec.HasActiveContract && rec.RemainingQuantity > 0)
                {
                    rec.Recommendation = (i == 0)
                        ? (rec.TotalFeedbackCount > 0
                            ? $"Top Recommended: Active contract with {rec.RemainingQuantity:N0} remaining capacity & {rec.AverageRating:0.0} quality score across {rec.TotalFeedbackCount} verified orders."
                            : $"Recommended: Priority active contract with {rec.RemainingQuantity:N0} remaining allocation at Rs. {rec.UnitPrice:N2}.")
                        : "Active Contract";
                }
                else
                {
                    if (rec.UnitPrice == minPrice)
                    {
                        rec.Recommendation = rec.TotalFeedbackCount > 0
                            ? $"Alternative — Best Price: Lowest unit price (Rs. {rec.UnitPrice:N2}) with {rec.AverageRating:0.0} customer rating."
                            : "Alternative — Best Price";
                    }
                    else if (rec.ContractID.HasValue && rec.RemainingQuantity <= 0)
                    {
                        rec.Recommendation = "Alternative (Capacity Depleted)";
                    }
                    else
                    {
                        rec.Recommendation = rec.TotalFeedbackCount > 0 && rec.AverageQualityRating >= 4.0m
                            ? $"Alternative: High quality rating ({rec.AverageQualityRating:0.0}/5) across {rec.TotalFeedbackCount} past deliveries."
                            : "Alternative";
                    }
                }
            }

            recommendations.AddRange(productRecommendations);
        }

        return new Response
        {
            PurchaseRequestID = request.PurchaseRequestID,
            Recommendations = recommendations
        };
    }
}
