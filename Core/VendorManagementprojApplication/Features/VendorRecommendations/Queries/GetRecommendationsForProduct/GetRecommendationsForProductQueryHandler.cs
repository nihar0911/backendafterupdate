using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using VendorManagementprojApplication.Contracts.Persistence;
using VendorManagementprojApplication.Contracts.Services;
using VendorManagementprojApplication.DTOs;

namespace VendorManagementprojApplication.Features.VendorRecommendations.Queries.GetRecommendationsForProduct;

public class GetRecommendationsForProductQueryHandler : IRequestHandler<GetRecommendationsForProductQuery, GetRecommendationsForProductResponse>
{
    private readonly IContractRepository _contractRepository;
    private readonly IVendorProductRepository _vendorProductRepository;
    private readonly IVendorRepository _vendorRepository;
    private readonly IVendorFeedbackRepository _vendorFeedbackRepository;
    private readonly IOutletRepository _outletRepository;
    private readonly IProductRepository _productRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly IVendorRecommendationSettingsRepository _settingsRepository;

    public GetRecommendationsForProductQueryHandler(
        IContractRepository contractRepository,
        IVendorProductRepository vendorProductRepository,
        IVendorRepository vendorRepository,
        IVendorFeedbackRepository vendorFeedbackRepository,
        IOutletRepository outletRepository,
        IProductRepository productRepository,
        ICurrentUserService currentUserService,
        IVendorRecommendationSettingsRepository settingsRepository)
    {
        _contractRepository = contractRepository;
        _vendorProductRepository = vendorProductRepository;
        _vendorRepository = vendorRepository;
        _vendorFeedbackRepository = vendorFeedbackRepository;
        _outletRepository = outletRepository;
        _productRepository = productRepository;
        _currentUserService = currentUserService;
        _settingsRepository = settingsRepository;
    }

    public async Task<GetRecommendationsForProductResponse> Handle(
        GetRecommendationsForProductQuery request,
        CancellationToken cancellationToken)
    {
        if (request.OutletID <= 0)
            throw new InvalidOperationException("A valid outlet ID is required.");

        if (request.ProductID <= 0)
            throw new InvalidOperationException("A valid product ID is required.");

        var outlet = await _outletRepository.GetByIdAsync(request.OutletID);
        if (outlet == null)
            throw new InvalidOperationException("Outlet does not exist.");

        var product = await _productRepository.GetByIdAsync(request.ProductID);
        if (product == null)
            throw new InvalidOperationException("Product does not exist.");

        // Authorization check
        if ((_currentUserService.IsOutletManager || _currentUserService.IsPurchaseManager) && _currentUserService.OutletID.HasValue)
        {
            if (request.OutletID != _currentUserService.OutletID.Value)
            {
                throw new UnauthorizedAccessException("You are not authorized to view recommendations for another outlet.");
            }
        }
        else if (_currentUserService.IsPurchaseManager && !_currentUserService.OutletID.HasValue)
        {
            throw new UnauthorizedAccessException("You are not assigned to an outlet.");
        }
        else if (_currentUserService.IsOrganizationManager && _currentUserService.OrganizationID.HasValue)
        {
            if (outlet.OrganizationID != _currentUserService.OrganizationID.Value)
            {
                throw new UnauthorizedAccessException("You are not authorized to view recommendations for an outlet outside your organization.");
            }
        }

        var settings = await _settingsRepository.GetSettingsAsync(cancellationToken);

        var allVendors = (await _vendorRepository.GetAllAsync())
            .Where(v => string.Equals(v.Status, "Active", StringComparison.OrdinalIgnoreCase))
            .ToDictionary(v => v.VendorID, v => v.VendorName);

        // =========================================================================
        // STEP 1: Determine whether active contracts exist for Outlet + Product
        // =========================================================================
        var activeContracts = await _contractRepository.GetActiveContractsByProductAndOutletAsync(
            request.OutletID,
            request.ProductID);

        var activeVendorProducts = await _vendorProductRepository.GetEligibleVendorsForProductAsync(
            request.ProductID,
            request.OutletID);

        // =========================================================================
        // CASE A: Active Contracts Found (YES) -> Return contracted vendors
        // =========================================================================
        if (activeContracts.Count > 0)
        {
            var contractedRecommendations = new List<VendorRecommendationDto>();
            decimal minContractPrice = decimal.MaxValue;

            // Pre-calculate prices to determine minPrice among contracted vendors
            foreach (var contract in activeContracts)
            {
                int vendorId = contract.VendorID ?? contract.VendorAllocations.FirstOrDefault()?.VendorID ?? 0;
                var cp = contract.ContractProducts.FirstOrDefault(p => p.ProductID == request.ProductID);
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

                var cp = contract.ContractProducts.FirstOrDefault(p => p.ProductID == request.ProductID);
                var vp = activeVendorProducts.FirstOrDefault(v => v.VendorID == vendorId);

                decimal unitPrice = cp?.UnitPrice ?? vp?.UnitPrice ?? 0m;
                int deliveryDays = vp?.EstimatedDeliveryDays ?? 1;

                decimal contractQty = cp?.ContractQuantity ?? contract.TotalQuantity;
                decimal purchasedQty = cp?.PurchasedQuantity ?? contract.UsedQuantity;
                decimal remainingQty = Math.Max(0m, contractQty - purchasedQty);

                // Retrieve historical reviews and feedback for this vendor
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

                // Composite Multi-Factor Score (0 - 100) using established formulas
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
                    ProductID = request.ProductID,
                    ProductName = product.ProductName,
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
                    ContractTotalQuantity = contractQty,
                    ContractStartDate = contract.StartDate,
                    ContractEndDate = contract.EndDate,
                    ContractStatus = contract.Status,
                    Recommendation = string.Empty,
                    SmartBadge = string.Empty
                });
            }

            // Rank contracted vendors among themselves
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

            return new GetRecommendationsForProductResponse
            {
                OutletID = request.OutletID,
                ProductID = request.ProductID,
                HasActiveContracts = true,
                Recommendations = rankedContractVendors
            };
        }

        // =========================================================================
        // CASE B: No Contract Fallback (NO) -> Existing recommendation system
        // =========================================================================
        if (!activeVendorProducts.Any())
        {
            return new GetRecommendationsForProductResponse
            {
                OutletID = request.OutletID,
                ProductID = request.ProductID,
                HasActiveContracts = false,
                Recommendations = new List<VendorRecommendationDto>()
            };
        }

        decimal minFallbackPrice = activeVendorProducts.Min(vp => vp.UnitPrice);
        var fallbackRecommendations = new List<VendorRecommendationDto>();

        foreach (var vendorProduct in activeVendorProducts)
        {
            string vendorName = allVendors.TryGetValue(vendorProduct.VendorID, out var name)
                ? name
                : $"Vendor #{vendorProduct.VendorID}";

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

            decimal qualityScore = feedbackCount > 0 ? (avgQuality / 5.0m) * 100m : settings.NeutralScoreForNewVendors;
            decimal deliveryScore = feedbackCount > 0 ? (avgDelivery / 5.0m) * 100m : settings.NeutralScoreForNewVendors;
            decimal priceScore = (vendorProduct.UnitPrice > 0 && minFallbackPrice > 0)
                ? Math.Round((minFallbackPrice / vendorProduct.UnitPrice) * 100m, 1)
                : settings.NeutralScoreForNewVendors;
            decimal reliabilityBonus = Math.Min(settings.ReliabilityWeight, feedbackCount * settings.ReliabilityPointsPerReview);

            decimal overallCompositeScore = Math.Round(
                (qualityScore * (settings.QualityWeight / 100m)) +
                (deliveryScore * (settings.DeliveryWeight / 100m)) +
                (priceScore * (settings.PriceWeight / 100m)) +
                reliabilityBonus,
                1
            );

            fallbackRecommendations.Add(new VendorRecommendationDto
            {
                VendorID = vendorProduct.VendorID,
                VendorName = vendorName,
                ProductID = request.ProductID,
                ProductName = product.ProductName,
                UnitPrice = vendorProduct.UnitPrice,
                EstimatedDeliveryDays = vendorProduct.EstimatedDeliveryDays,
                OverallScore = overallCompositeScore,
                AverageRating = avgRating,
                AverageQualityRating = avgQuality,
                AverageDeliveryRating = avgDelivery,
                TotalFeedbackCount = feedbackCount,
                DeliveryCompletionPercentage = deliveryScore,
                AverageSpoilagePercentage = 0m,
                HasActiveContract = false,
                Recommendation = string.Empty,
                SmartBadge = string.Empty
            });
        }

        var rankedFallback = fallbackRecommendations
            .OrderByDescending(r => r.OverallScore)
            .ThenBy(r => r.UnitPrice)
            .ThenBy(r => r.EstimatedDeliveryDays)
            .ThenBy(r => r.VendorID)
            .ToList();

        for (int i = 0; i < rankedFallback.Count; i++)
        {
            var rec = rankedFallback[i];
            rec.Rank = i + 1;

            if (i == 0)
            {
                rec.SmartBadge = "Top Recommended";
            }
            else if (rec.AverageQualityRating >= settings.BestQualityThreshold)
            {
                rec.SmartBadge = "Best Quality";
            }
            else if (rec.UnitPrice == minFallbackPrice)
            {
                rec.SmartBadge = "Best Price";
            }
            else if (rec.AverageDeliveryRating >= settings.FastestDeliveryThreshold)
            {
                rec.SmartBadge = "Fastest Delivery";
            }

            if (rec.UnitPrice == minFallbackPrice)
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

        return new GetRecommendationsForProductResponse
        {
            OutletID = request.OutletID,
            ProductID = request.ProductID,
            HasActiveContracts = false,
            Recommendations = rankedFallback
        };
    }
}
