using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using VendorManagementproj.Application.Contracts.Persistence;
using VendorManagementproj.Application.Contracts.Services;
using VendorManagementproj.Application.DTOs;

namespace VendorManagementproj.Application.Features.Contracts.Queries.GetContractEligibleVendors;

public class GetContractEligibleVendorsQueryHandler : IRequestHandler<GetContractEligibleVendorsQuery, GetContractEligibleVendorsResponse>
{
    private readonly IVendorProductRepository _vendorProductRepository;
    private readonly IContractRepository _contractRepository;
    private readonly IVendorRepository _vendorRepository;
    private readonly IVendorFeedbackRepository _vendorFeedbackRepository;
    private readonly IOutletRepository _outletRepository;
    private readonly IProductRepository _productRepository;
    private readonly ICurrentUserService _currentUserService;

    public GetContractEligibleVendorsQueryHandler(
        IVendorProductRepository vendorProductRepository,
        IContractRepository contractRepository,
        IVendorRepository vendorRepository,
        IVendorFeedbackRepository vendorFeedbackRepository,
        IOutletRepository outletRepository,
        IProductRepository productRepository,
        ICurrentUserService currentUserService)
    {
        _vendorProductRepository = vendorProductRepository;
        _contractRepository = contractRepository;
        _vendorRepository = vendorRepository;
        _vendorFeedbackRepository = vendorFeedbackRepository;
        _outletRepository = outletRepository;
        _productRepository = productRepository;
        _currentUserService = currentUserService;
    }

    public async Task<GetContractEligibleVendorsResponse> Handle(
        GetContractEligibleVendorsQuery request,
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
        if (_currentUserService.IsOrganizationManager && _currentUserService.OrganizationID.HasValue)
        {
            if (outlet.OrganizationID != _currentUserService.OrganizationID.Value)
            {
                throw new UnauthorizedAccessException("You are not authorized to view eligible vendors for an outlet outside your organization.");
            }
        }
        else if ((_currentUserService.IsOutletManager || _currentUserService.IsPurchaseManager) && _currentUserService.OutletID.HasValue)
        {
            if (request.OutletID != _currentUserService.OutletID.Value)
            {
                throw new UnauthorizedAccessException("You are not authorized to view eligible vendors for another outlet.");
            }
        }

        // 1. Check existing active contracts for (OutletID, ProductID)
        var activeContracts = await _contractRepository.GetActiveContractsByProductAndOutletAsync(
            request.OutletID,
            request.ProductID);

        bool hasActiveContracts = activeContracts.Count > 0;

        var contractedVendorIds = activeContracts
            .Select(c => c.VendorID ?? c.VendorAllocations.FirstOrDefault()?.VendorID ?? 0)
            .Where(id => id > 0)
            .ToHashSet();

        // 2. Retrieve ALL eligible vendor products actively supplying this product
        var activeVendorProducts = await _vendorProductRepository.GetEligibleVendorsForProductAsync(
            request.ProductID,
            request.OutletID);

        // 3. Retrieve all active vendors dictionary
        var allVendors = (await _vendorRepository.GetAllAsync())
            .Where(v => string.Equals(v.Status, "Active", StringComparison.OrdinalIgnoreCase))
            .ToDictionary(v => v.VendorID, v => v.VendorName);

        var resultVendors = new List<VendorRecommendationDto>();

        foreach (var vp in activeVendorProducts)
        {
            int vendorId = vp.VendorID;
            string vendorName = allVendors.TryGetValue(vendorId, out var name)
                ? name
                : $"Vendor #{vendorId}";

            bool hasContract = contractedVendorIds.Contains(vendorId);
            var matchingContract = activeContracts.FirstOrDefault(c => (c.VendorID ?? c.VendorAllocations.FirstOrDefault()?.VendorID) == vendorId);

            var feedbacks = await _vendorFeedbackRepository.GetByVendorIdAsync(vendorId);
            int feedbackCount = feedbacks?.Count ?? 0;
            decimal avgRating = feedbackCount > 0 ? Math.Round(feedbacks!.Average(f => f.Rating), 1) : 0m;
            decimal avgQuality = feedbackCount > 0 ? Math.Round(feedbacks!.Average(f => f.ProductQualityRating), 1) : 0m;
            decimal avgDelivery = feedbackCount > 0 ? Math.Round(feedbacks!.Average(f => f.DeliveryRating), 1) : 0m;

            var cp = matchingContract?.ContractProducts.FirstOrDefault(p => p.ProductID == request.ProductID);
            decimal contractQty = cp != null 
                ? cp.ContractQuantity 
                : (matchingContract != null && (!matchingContract.ContractProducts.Any()) ? matchingContract.TotalQuantity : 0m);
            decimal purchasedQty = cp != null 
                ? cp.PurchasedQuantity 
                : (matchingContract != null && (!matchingContract.ContractProducts.Any()) ? matchingContract.UsedQuantity : 0m);
            decimal remainingQty = Math.Max(0m, contractQty - purchasedQty);
            decimal unitPrice = cp?.UnitPrice ?? vp.UnitPrice;
            decimal contractTotalQty = matchingContract != null
                ? (matchingContract.ContractProducts.Any() ? matchingContract.ContractProducts.Sum(p => p.ContractQuantity) : matchingContract.TotalQuantity)
                : 0m;

            resultVendors.Add(new VendorRecommendationDto
            {
                VendorID = vendorId,
                VendorName = vendorName,
                ProductID = request.ProductID,
                ProductName = product.ProductName,
                UnitPrice = unitPrice,
                EstimatedDeliveryDays = vp.EstimatedDeliveryDays,
                AverageRating = avgRating,
                AverageQualityRating = avgQuality,
                AverageDeliveryRating = avgDelivery,
                TotalFeedbackCount = feedbackCount,
                HasActiveContract = hasContract,
                ContractID = matchingContract?.ContractID,
                ContractQuantity = matchingContract != null ? contractQty : null,
                PurchasedQuantity = matchingContract != null ? purchasedQty : null,
                AllocatedQuantity = matchingContract != null ? contractQty : 0m,
                UsedQuantity = matchingContract != null ? purchasedQty : 0m,
                RemainingQuantity = remainingQty,
                ContractTotalQuantity = contractTotalQty,
                ContractStartDate = matchingContract?.StartDate,
                ContractEndDate = matchingContract?.EndDate,
                ContractStatus = matchingContract?.Status
            });
        }

        // Also check if any vendor has an active contract but was not in activeVendorProducts
        foreach (var contract in activeContracts)
        {
            int vendorId = contract.VendorID ?? contract.VendorAllocations.FirstOrDefault()?.VendorID ?? 0;
            if (vendorId > 0 && !resultVendors.Any(v => v.VendorID == vendorId))
            {
                string vendorName = contract.Vendor?.VendorName
                    ?? (allVendors.TryGetValue(vendorId, out var name) ? name : $"Vendor #{vendorId}");

                var cp = contract.ContractProducts.FirstOrDefault(p => p.ProductID == request.ProductID);
                decimal unitPrice = cp?.UnitPrice ?? 0m;

                var feedbacks = await _vendorFeedbackRepository.GetByVendorIdAsync(vendorId);
                int feedbackCount = feedbacks?.Count ?? 0;
                decimal avgRating = feedbackCount > 0 ? Math.Round(feedbacks!.Average(f => f.Rating), 1) : 0m;
                decimal avgQuality = feedbackCount > 0 ? Math.Round(feedbacks!.Average(f => f.ProductQualityRating), 1) : 0m;
                decimal avgDelivery = feedbackCount > 0 ? Math.Round(feedbacks!.Average(f => f.DeliveryRating), 1) : 0m;

                decimal contractQty = cp?.ContractQuantity ?? (!contract.ContractProducts.Any() ? contract.TotalQuantity : 0m);
                decimal purchasedQty = cp?.PurchasedQuantity ?? (!contract.ContractProducts.Any() ? contract.UsedQuantity : 0m);
                decimal remainingQty = Math.Max(0m, contractQty - purchasedQty);
                decimal contractTotalQty = contract.ContractProducts.Any()
                    ? contract.ContractProducts.Sum(p => p.ContractQuantity)
                    : contract.TotalQuantity;

                resultVendors.Add(new VendorRecommendationDto
                {
                    VendorID = vendorId,
                    VendorName = vendorName,
                    ProductID = request.ProductID,
                    ProductName = product.ProductName,
                    UnitPrice = unitPrice,
                    EstimatedDeliveryDays = 1,
                    AverageRating = avgRating,
                    AverageQualityRating = avgQuality,
                    AverageDeliveryRating = avgDelivery,
                    TotalFeedbackCount = feedbackCount,
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
                    ContractStatus = contract.Status
                });
            }
        }

        var orderedVendors = resultVendors.OrderBy(v => v.VendorName).ToList();

        return new GetContractEligibleVendorsResponse
        {
            OutletID = request.OutletID,
            ProductID = request.ProductID,
            HasActiveContracts = hasActiveContracts,
            Vendors = orderedVendors
        };
    }
}
