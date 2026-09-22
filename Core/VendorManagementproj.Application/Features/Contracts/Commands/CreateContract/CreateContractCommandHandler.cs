using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using VendorManagementproj.Application.Contracts.Persistence;
using VendorManagementproj.Application.Contracts.Services;
using VendorManagementproj.Application.DTOs;
using VendorManagementproj.Domain.Entities;

namespace VendorManagementproj.Application.Features.Contracts.Commands.CreateContract;

public class CreateContractCommandHandler : IRequestHandler<CreateContractCommand, CreateContractResponse>
{
    private readonly IContractRepository _contractRepository;
    private readonly IVendorProductRepository _vendorProductRepository;
    private readonly IVendorRepository _vendorRepository;
    private readonly IProductRepository _productRepository;
    private readonly IOutletRepository _outletRepository;
    private readonly ICurrentUserService _currentUserService;

    public CreateContractCommandHandler(
        IContractRepository contractRepository,
        IVendorProductRepository vendorProductRepository,
        IVendorRepository vendorRepository,
        IProductRepository productRepository,
        IOutletRepository outletRepository,
        ICurrentUserService currentUserService)
    {
        _contractRepository = contractRepository;
        _vendorProductRepository = vendorProductRepository;
        _vendorRepository = vendorRepository;
        _productRepository = productRepository;
        _outletRepository = outletRepository;
        _currentUserService = currentUserService;
    }

    public async Task<CreateContractResponse> Handle(
        CreateContractCommand request,
        CancellationToken cancellationToken)
    {
        if (request.EndDate <= request.StartDate)
            throw new InvalidOperationException("End date must be after start date.");

        var targetOutlet = await _outletRepository.GetByIdAsync(request.OutletID);
        if (targetOutlet == null)
            throw new InvalidOperationException("Target outlet does not exist.");

        if (!string.Equals(targetOutlet.Status, "Active", StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException($"Outlet '{targetOutlet.OutletName}' (ID #{request.OutletID}) is not active.");

        // Authorization check
        if (_currentUserService.IsOrganizationManager)
        {
            if (!_currentUserService.OrganizationID.HasValue ||
                targetOutlet.OrganizationID != _currentUserService.OrganizationID.Value)
            {
                throw new UnauthorizedAccessException(
                    "You cannot create a contract for an outlet outside your organization.");
            }
        }
        else if (_currentUserService.IsOutletManager)
        {
            if (!_currentUserService.OutletID.HasValue ||
                request.OutletID != _currentUserService.OutletID.Value)
            {
                throw new UnauthorizedAccessException(
                    "You cannot create a contract for another outlet.");
            }
        }
        else if (!_currentUserService.IsAdmin)
        {
            throw new UnauthorizedAccessException(
                "You are not authorized to create a contract.");
        }

        // Resolve items (target multi-product format vs legacy allocation format)
        var resolvedItems = new List<ContractProductAssignmentDto>();

        if (request.Assignments != null && request.Assignments.Count > 0)
        {
            resolvedItems.AddRange(request.Assignments);
        }
        else if (request.Items != null && request.Items.Count > 0)
        {
            resolvedItems.AddRange(request.Items);
        }
        else if (request.ProductID > 0 && request.TotalQuantity > 0)
        {
            // Backward-compatible fallback for legacy payloads
            if (request.Allocations != null && request.Allocations.Count > 0)
            {
                var positiveAllocations = request.Allocations.Where(x => x.AllocationPercentage > 0).ToList();
                if (positiveAllocations.Count == 0)
                    throw new InvalidOperationException("At least one vendor must have a positive allocation percentage.");

                foreach (var alloc in positiveAllocations)
                {
                    var qty = Math.Round(request.TotalQuantity * alloc.AllocationPercentage / 100m, 2, MidpointRounding.AwayFromZero);
                    resolvedItems.Add(new ContractProductAssignmentDto
                    {
                        ProductID = request.ProductID,
                        VendorID = alloc.VendorID,
                        ContractQuantity = qty
                    });
                }
            }
        }

        if (resolvedItems.Count == 0)
            throw new InvalidOperationException("At least one product assignment is required.");

        // Validate items
        foreach (var item in resolvedItems)
        {
            if (item.ProductID <= 0)
                throw new InvalidOperationException("A valid product ID is required.");

            if (item.VendorID <= 0)
                throw new InvalidOperationException("A valid vendor ID is required.");

            if (item.ContractQuantity <= 0)
                throw new InvalidOperationException("Contract quantity must be greater than zero.");

            var product = await _productRepository.GetByIdAsync(item.ProductID);
            if (product == null)
                throw new InvalidOperationException($"Product ID #{item.ProductID} does not exist.");

            if (!string.Equals(product.Status, "Active", StringComparison.OrdinalIgnoreCase))
                throw new InvalidOperationException($"Product '{product.ProductName}' (ID #{item.ProductID}) is not active.");

            var vendor = await _vendorRepository.GetByIdAsync(item.VendorID);
            if (vendor == null)
                throw new InvalidOperationException($"Vendor ID #{item.VendorID} does not exist.");

            if (!string.Equals(vendor.Status, "Active", StringComparison.OrdinalIgnoreCase))
                throw new InvalidOperationException($"Vendor '{vendor.VendorName}' (ID #{item.VendorID}) is not active.");

            var vendorProduct = await _vendorProductRepository.GetByVendorAndProductAsync(item.VendorID, item.ProductID);
            if (vendorProduct == null || !string.Equals(vendorProduct.Status, "Active", StringComparison.OrdinalIgnoreCase))
                throw new InvalidOperationException($"Vendor '{vendor.VendorName}' does not actively supply product '{product.ProductName}'.");

            if (!item.UnitPrice.HasValue || item.UnitPrice.Value <= 0)
            {
                item.UnitPrice = vendorProduct.UnitPrice;
            }
        }

        // Check for duplicate assignments within the current request
        if (resolvedItems.GroupBy(x => new { x.VendorID, x.ProductID }).Any(g => g.Count() > 1))
        {
            var duplicate = resolvedItems.GroupBy(x => new { x.VendorID, x.ProductID }).First(g => g.Count() > 1);
            var v = await _vendorRepository.GetByIdAsync(duplicate.Key.VendorID);
            var p = await _productRepository.GetByIdAsync(duplicate.Key.ProductID);
            var vName = v?.VendorName ?? $"#{duplicate.Key.VendorID}";
            var pName = p?.ProductName ?? $"#{duplicate.Key.ProductID}";
            throw new InvalidOperationException(
                $"Duplicate assignment for Vendor '{vName}' and Product '{pName}' in the same contract request.");
        }

        // Duplicate Active Overlapping Contract Rule:
        // Reject overlapping active contracts for the exact (Outlet + Vendor + Product) combination
        // Consistent with GetActiveContractsByProductAndOutletAsync: must be Active and currently valid at DateTime.Now
        var allContracts = await _contractRepository.GetAllAsync();
        var now = DateTime.Now;

        foreach (var item in resolvedItems)
        {
            var overlappingContract = allContracts.FirstOrDefault(c =>
                c.OutletID == request.OutletID &&
                (c.VendorID == item.VendorID || c.VendorAllocations.Any(va => va.VendorID == item.VendorID && string.Equals(va.Status, "Active", StringComparison.OrdinalIgnoreCase))) &&
                string.Equals(c.Status, "Active", StringComparison.OrdinalIgnoreCase) &&
                c.EndDate >= now &&
                c.StartDate < request.EndDate &&
                c.EndDate > request.StartDate &&
                (c.ContractProducts.Any(cp => cp.ProductID == item.ProductID) || (!c.ContractProducts.Any() && c.ProductID == item.ProductID)));

            if (overlappingContract != null)
            {
                var v = await _vendorRepository.GetByIdAsync(item.VendorID);
                var p = await _productRepository.GetByIdAsync(item.ProductID);
                var vName = v?.VendorName ?? $"#{item.VendorID}";
                var pName = p?.ProductName ?? $"#{item.ProductID}";
                throw new InvalidOperationException(
                    $"An active contract already exists for Outlet ID {request.OutletID}, Vendor '{vName}' and Product '{pName}' covering an overlapping validity period ({overlappingContract.StartDate:yyyy-MM-dd} to {overlappingContract.EndDate:yyyy-MM-dd}).");
            }
        }

        var paymentMethod = !string.IsNullOrWhiteSpace(request.PaymentMethod) ? request.PaymentMethod : "Net30";

        // Group products by VendorID: ONE Contract per Vendor
        var contractsToCreate = new List<Contract>();
        foreach (var vendorGroup in resolvedItems.GroupBy(x => x.VendorID))
        {
            int vendorId = vendorGroup.Key;
            var groupItems = vendorGroup.ToList();
            var firstItem = groupItems.First();
            decimal totalQty = groupItems.Sum(x => x.ContractQuantity);

            var contract = new Contract
            {
                QuotationID = request.QuotationID,
                OutletID = request.OutletID,
                VendorID = vendorId,
                ProductID = firstItem.ProductID, // Legacy field
                TotalQuantity = totalQty,        // Legacy field
                UsedQuantity = 0,               // Legacy field
                StartDate = request.StartDate,
                EndDate = request.EndDate,
                PaymentMethod = paymentMethod,
                Status = "Active",
                ContractProducts = groupItems.Select(gi => new ContractProduct
                {
                    ProductID = gi.ProductID,
                    ContractQuantity = gi.ContractQuantity,
                    PurchasedQuantity = 0,
                    UnitPrice = gi.UnitPrice
                }).ToList(),
                VendorAllocations = new List<ContractVendorAllocation>
                {
                    new ContractVendorAllocation
                    {
                        VendorID = vendorId,
                        AllocationPercentage = 100m,
                        AllocatedQuantity = totalQty,
                        UsedQuantity = 0,
                        Status = "Active"
                    }
                }
            };

            contractsToCreate.Add(contract);
        }

        // Save all contracts in ONE atomic database transaction via repository
        await _contractRepository.AddBatchAsync(contractsToCreate);

        // Cache products and vendors for DTO mapping
        var allProductIds = resolvedItems.Select(i => i.ProductID).Distinct().ToList();
        var allVendorIds = resolvedItems.Select(i => i.VendorID).Distinct().ToList();
        var productDict = new Dictionary<int, Product>();
        foreach (var pid in allProductIds)
        {
            var p = await _productRepository.GetByIdAsync(pid);
            if (p != null) productDict[pid] = p;
        }

        var vendorDict = new Dictionary<int, Vendor>();
        foreach (var vid in allVendorIds)
        {
            var v = await _vendorRepository.GetByIdAsync(vid);
            if (v != null) vendorDict[vid] = v;
        }

        var createdDtos = contractsToCreate.Select(c =>
        {
            string vendorName = c.VendorID.HasValue && vendorDict.TryGetValue(c.VendorID.Value, out var v)
                ? v.VendorName
                : "Vendor";

            var firstCp = c.ContractProducts.FirstOrDefault();
            var firstP = firstCp != null && productDict.TryGetValue(firstCp.ProductID, out var fp) ? fp : null;

            return new ContractDto
            {
                ContractID = c.ContractID,
                QuotationID = c.QuotationID,
                OutletID = c.OutletID,
                OutletName = targetOutlet.OutletName,
                OrganizationID = targetOutlet.OrganizationID,
                OrganizationName = targetOutlet.Organization?.OrganizationName ?? "Organization",
                VendorID = c.VendorID,
                VendorName = vendorName,
                ProductID = firstCp?.ProductID ?? c.ProductID,
                ProductName = firstP?.ProductName ?? $"Product #{c.ProductID}",
                Unit = firstP?.Unit ?? "Kg",
                TotalQuantity = c.ContractProducts.Sum(cp => cp.ContractQuantity),
                UsedQuantity = c.ContractProducts.Sum(cp => cp.PurchasedQuantity),
                StartDate = c.StartDate,
                EndDate = c.EndDate,
                PaymentMethod = c.PaymentMethod,
                Status = c.Status,
                Products = c.ContractProducts.Select(cp =>
                {
                    productDict.TryGetValue(cp.ProductID, out var prod);
                    return new ContractProductDto
                    {
                        ContractProductID = cp.ContractProductID,
                        ContractID = cp.ContractID,
                        ProductID = cp.ProductID,
                        ProductName = prod?.ProductName ?? $"Product #{cp.ProductID}",
                        Unit = prod?.Unit ?? "Kg",
                        ContractQuantity = cp.ContractQuantity,
                        PurchasedQuantity = cp.PurchasedQuantity,
                        UnitPrice = cp.UnitPrice
                    };
                }).ToList(),
                Allocations = c.VendorAllocations.Select(a => new ContractVendorAllocationDto
                {
                    ContractVendorAllocationID = a.ContractVendorAllocationID,
                    VendorID = a.VendorID,
                    VendorName = vendorName,
                    AllocationPercentage = a.AllocationPercentage,
                    AllocatedQuantity = a.AllocatedQuantity,
                    UsedQuantity = a.UsedQuantity,
                    Status = a.Status
                }).ToList()
            };
        }).ToList();

        return new CreateContractResponse
        {
            Contracts = createdDtos,
            Contract = createdDtos.FirstOrDefault()!
        };
    }
}
