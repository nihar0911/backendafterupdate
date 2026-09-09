using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using VendorManagementprojApplication.Contracts.Infrastructure;
using VendorManagementprojApplication.Contracts.Persistence;
using VendorManagementprojApplication.Contracts.Services;
using VendorManagementprojApplication.DTOs;
using VendorManagementprojDomain.Entities;

namespace VendorManagementprojApplication.Features.DeliveryRecords.Queries.GetSpoilageAdvice;

public class GetSpoilageAdviceQueryHandler : IRequestHandler<GetSpoilageAdviceQuery, GetSpoilageAdviceResponse>
{
    private readonly IPurchaseOrderRepository _purchaseOrderRepository;
    private readonly IDeliveryRecordRepository _deliveryRecordRepository;
    private readonly IVendorRepository _vendorRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly IGeminiAiService _geminiAiService;

    public GetSpoilageAdviceQueryHandler(
        IPurchaseOrderRepository purchaseOrderRepository,
        IDeliveryRecordRepository deliveryRecordRepository,
        IVendorRepository vendorRepository,
        ICurrentUserService currentUserService,
        IGeminiAiService geminiAiService)
    {
        _purchaseOrderRepository = purchaseOrderRepository;
        _deliveryRecordRepository = deliveryRecordRepository;
        _vendorRepository = vendorRepository;
        _currentUserService = currentUserService;
        _geminiAiService = geminiAiService;
    }

    public async Task<GetSpoilageAdviceResponse> Handle(
        GetSpoilageAdviceQuery request,
        CancellationToken cancellationToken)
    {
        var purchaseOrder = await _purchaseOrderRepository.GetByIdAsync(request.PurchaseOrderID);
        if (purchaseOrder == null)
        {
            throw new InvalidOperationException($"Purchase Order #{request.PurchaseOrderID} does not exist.");
        }

        // Security / Vendor Isolation Check
        if (_currentUserService.IsVendorManager)
        {
            if (!_currentUserService.VendorID.HasValue)
            {
                throw new UnauthorizedAccessException("Your vendor account is not properly configured. Access denied.");
            }

            if (purchaseOrder.VendorID != _currentUserService.VendorID.Value)
            {
                throw new UnauthorizedAccessException("You are not authorized to view spoilage advice for another vendor's purchase order.");
            }
        }
        else if (_currentUserService.IsOutletManager || _currentUserService.IsPurchaseManager)
        {
            if (_currentUserService.OutletID.HasValue && purchaseOrder.OutletID != _currentUserService.OutletID.Value)
            {
                throw new UnauthorizedAccessException("You are not authorized to view purchase orders for another outlet.");
            }
        }

        int vendorId = purchaseOrder.VendorID;
        string vendorName = purchaseOrder.Vendor?.VendorName ?? string.Empty;
        if (string.IsNullOrWhiteSpace(vendorName))
        {
            var vendor = await _vendorRepository.GetByIdAsync(vendorId);
            vendorName = vendor?.VendorName ?? $"Vendor #{vendorId}";
        }

        var confirmedDeliveries = await _deliveryRecordRepository.GetConfirmedByVendorAsync(vendorId);
        var poItems = purchaseOrder.Items?.ToList() ?? new List<PurchaseOrderItem>();

        var productAdvices = new List<ProductSpoilageAdviceDto>();

        foreach (var item in poItems)
        {
            int productId = item.ProductID;
            string productName = item.Product?.ProductName ?? $"Product #{productId}";
            string unit = item.Product?.Unit ?? string.Empty;
            decimal currentQty = item.Quantity;

            var productDeliveries = confirmedDeliveries
                .Where(d => d.PurchaseOrderItem != null && d.PurchaseOrderItem.ProductID == productId)
                .OrderByDescending(d => d.DeliveryDate)
                .ToList();

            int deliveryCount = productDeliveries.Count;

            var advice = new ProductSpoilageAdviceDto
            {
                ProductID = productId,
                ProductName = productName,
                Unit = unit,
                POItemID = item.POItemID,
                CurrentQuantity = currentQty,
                DeliveryCount = deliveryCount
            };

            if (deliveryCount == 0)
            {
                // Cold Start / Insufficient Data
                advice.TotalOrderedQuantity = 0;
                advice.TotalReceivedQuantity = 0;
                advice.TotalSpoiledQuantity = 0;
                advice.WeightedSpoilagePercentage = null;
                advice.AverageSpoilagePercentage = null;
                advice.RecentSpoilagePercentage = null;
                advice.MinimumSpoilagePercentage = null;
                advice.MaximumSpoilagePercentage = null;
                advice.Trend = "None";
                advice.RiskLevel = "INSUFFICIENT DATA";
                advice.EstimatedSpoiledQuantity = null;
            }
            else
            {
                decimal totalOrdered = productDeliveries.Sum(d => d.OrderedQuantity);
                decimal totalReceived = productDeliveries.Sum(d => d.ReceivedQuantity);
                decimal totalSpoiled = productDeliveries.Sum(d => d.SpoiledQuantity);

                decimal weightedSpoilage = totalReceived > 0
                    ? Math.Round((totalSpoiled / totalReceived) * 100m, 2)
                    : 0m;

                decimal avgSpoilage = Math.Round(productDeliveries.Average(d => d.SpoilagePercentage), 2);
                decimal minSpoilage = Math.Round(productDeliveries.Min(d => d.SpoilagePercentage), 2);
                decimal maxSpoilage = Math.Round(productDeliveries.Max(d => d.SpoilagePercentage), 2);

                // Recent deliveries (last 3-5 deliveries)
                var recentDeliveries = productDeliveries.Take(5).ToList();
                decimal recentReceived = recentDeliveries.Sum(d => d.ReceivedQuantity);
                decimal recentSpoiled = recentDeliveries.Sum(d => d.SpoiledQuantity);
                decimal recentSpoilage = recentReceived > 0
                    ? Math.Round((recentSpoiled / recentReceived) * 100m, 2)
                    : weightedSpoilage;

                // Trend Calculation
                string trend;
                if (recentSpoilage > weightedSpoilage + 1.0m)
                {
                    trend = "Increasing";
                }
                else if (recentSpoilage < weightedSpoilage - 1.0m)
                {
                    trend = "Decreasing";
                }
                else
                {
                    trend = "Stable";
                }

                // Risk Level Rules
                string riskLevel;
                if (weightedSpoilage >= 5.0m || recentSpoilage >= 6.0m || maxSpoilage >= 10.0m)
                {
                    riskLevel = "HIGH";
                }
                else if (weightedSpoilage >= 2.0m && weightedSpoilage < 5.0m)
                {
                    riskLevel = "MEDIUM";
                }
                else if (weightedSpoilage < 2.0m && (trend == "Stable" || trend == "Decreasing"))
                {
                    riskLevel = "LOW";
                }
                else
                {
                    riskLevel = "MEDIUM";
                }
                //based on past spoilage percentage, what are the likely chances that the current quantity will  spoil before it can be dispatched

                decimal? estimatedSpoiled = totalReceived > 0
                    ? Math.Round(currentQty * (weightedSpoilage / 100m), 2)
                    : null;

                advice.TotalOrderedQuantity = totalOrdered;
                advice.TotalReceivedQuantity = totalReceived;
                advice.TotalSpoiledQuantity = totalSpoiled;
                advice.WeightedSpoilagePercentage = weightedSpoilage;
                advice.AverageSpoilagePercentage = avgSpoilage;
                advice.RecentSpoilagePercentage = recentSpoilage;
                advice.MinimumSpoilagePercentage = minSpoilage;
                advice.MaximumSpoilagePercentage = maxSpoilage;
                advice.Trend = trend;
                advice.RiskLevel = riskLevel;
                advice.EstimatedSpoiledQuantity = estimatedSpoiled;
            }

            productAdvices.Add(advice);
        }

        bool isSingleProduct = productAdvices.Count <= 1;

        if (isSingleProduct)
        {
            if (productAdvices.Count == 1)
            {
                productAdvices[0].PriorityRank = null; // Do not return ranking for single product
            }
        }
        else
        {
            // Multi-product ordering & prioritization
            productAdvices = productAdvices
                .OrderBy(p => GetRiskSortOrder(p.RiskLevel))
                .ThenByDescending(p => p.WeightedSpoilagePercentage ?? -1)
                .ThenByDescending(p => p.CurrentQuantity)
                .ToList();

            int rank = 1;
            foreach (var prod in productAdvices)
            {
                prod.PriorityRank = rank++;
            }
        }

        // Generate AI Spoilage & Dispatch Narrative with full ranked PO context
        var aiResult = await _geminiAiService.GenerateSpoilageAdviceAsync(
            vendorName,
            purchaseOrder.PurchaseOrderID,
            isSingleProduct,
            productAdvices);

        foreach (var prod in productAdvices)
        {
            var narrative = aiResult.ProductNarratives.FirstOrDefault(n => n.ProductID == prod.ProductID);
            if (narrative != null)
            {
                prod.Why = narrative.Why;
                prod.RecommendedAction = narrative.RecommendedAction;
            }
        }

        string overallSummary = !string.IsNullOrWhiteSpace(aiResult.OverallSummary)
            ? aiResult.OverallSummary
            : (isSingleProduct
                ? (productAdvices.Count == 1 && productAdvices[0].RiskLevel == "INSUFFICIENT DATA"
                    ? $"Spoilage advice for {productAdvices[0].ProductName}: No previous confirmed delivery history available."
                    : (productAdvices.Count == 1
                        ? $"Spoilage risk for {productAdvices[0].ProductName} is {productAdvices[0].RiskLevel} based on {productAdvices[0].DeliveryCount} previous confirmed deliveries."
                        : "Purchase order has no items to analyze."))
                : $"{productAdvices.Count} products evaluated. Follow assigned dispatch order.");

        var advisorDto = new SpoilageAdvisorDto
        {
            PurchaseOrderID = purchaseOrder.PurchaseOrderID,
            VendorID = vendorId,
            VendorName = vendorName,
            IsSingleProduct = isSingleProduct,
            OverallSummary = overallSummary,
            Products = productAdvices
        };

        return new GetSpoilageAdviceResponse
        {
            Advisor = advisorDto
        };
    }
  private static int GetRiskSortOrder(string riskLevel)
    {
        return riskLevel?.ToUpperInvariant() switch
        {
            "HIGH" => 1,
            "MEDIUM" => 2,
            "LOW" => 3,
            "INSUFFICIENT DATA" => 4,
            _ => 5
        };
    }
}
