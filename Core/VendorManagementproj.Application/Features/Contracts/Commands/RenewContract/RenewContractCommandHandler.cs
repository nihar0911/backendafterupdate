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

namespace VendorManagementproj.Application.Features.Contracts.Commands.RenewContract;

public class RenewContractCommandHandler : IRequestHandler<RenewContractCommand, RenewContractResponse>
{
    private readonly IContractRepository _contractRepository;
    private readonly IVendorProductRepository _vendorProductRepository;
    private readonly IVendorRepository _vendorRepository;
    private readonly IProductRepository _productRepository;
    private readonly IOutletRepository _outletRepository;
    private readonly ICurrentUserService _currentUserService;

    public RenewContractCommandHandler(
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

    public async Task<RenewContractResponse> Handle(
        RenewContractCommand request,
        CancellationToken cancellationToken)
    {
        if (request.ContractID <= 0)
            throw new InvalidOperationException("A valid contract ID is required.");

        var oldContract = await _contractRepository.GetByIdAsync(request.ContractID);
        if (oldContract == null)
            throw new KeyNotFoundException($"Contract #{request.ContractID} does not exist.");

        // Authorization checks: Only Admin or Organization Manager (for their own organization)
        if (!_currentUserService.IsAdmin && !_currentUserService.IsOrganizationManager)
        {
            throw new UnauthorizedAccessException("Only an Organization Manager or Admin is authorized to renew contracts.");
        }

        if (_currentUserService.IsOrganizationManager)
        {
            if (!_currentUserService.OrganizationID.HasValue)
            {
                throw new UnauthorizedAccessException("Organization context is missing.");
            }

            var targetOutlet = oldContract.Outlet ?? await _outletRepository.GetByIdAsync(oldContract.OutletID);
            if (targetOutlet == null || targetOutlet.OrganizationID != _currentUserService.OrganizationID.Value)
            {
                throw new UnauthorizedAccessException("You cannot renew contracts for an outlet outside your organization.");
            }
        }

        // Prevent renewing an already ended contract
        if (string.Equals(oldContract.Status, "Ended", StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException($"Contract #{request.ContractID} has already been ended and cannot be renewed.");
        }

        // Prevent renewing an active (non-expired) contract
        var now = DateTime.Now;
        if (string.Equals(oldContract.Status, "Active", StringComparison.OrdinalIgnoreCase) && oldContract.EndDate >= now)
        {
            throw new InvalidOperationException($"Contract #{request.ContractID} is currently active and cannot be renewed until it has expired.");
        }

        // Date validation
        if (request.EndDate <= request.StartDate)
        {
            throw new InvalidOperationException("End date must be after start date.");
        }

        // Validate Outlet
        var outlet = oldContract.Outlet ?? await _outletRepository.GetByIdAsync(oldContract.OutletID);
        if (outlet == null)
            throw new InvalidOperationException("Target outlet does not exist.");

        if (!string.Equals(outlet.Status, "Active", StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException($"Outlet '{outlet.OutletName}' (ID #{outlet.OutletID}) is not active.");

        // Validate Vendor
        int? vendorId = oldContract.VendorID ?? oldContract.VendorAllocations?.FirstOrDefault()?.VendorID;
        if (!vendorId.HasValue || vendorId.Value <= 0)
            throw new InvalidOperationException("Original contract does not have an associated vendor.");

        var vendor = oldContract.Vendor ?? await _vendorRepository.GetByIdAsync(vendorId.Value);
        if (vendor == null)
            throw new InvalidOperationException($"Vendor ID #{vendorId.Value} does not exist.");

        if (!string.Equals(vendor.Status, "Active", StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException($"Vendor '{vendor.VendorName}' (ID #{vendorId.Value}) is not active.");

        // Resolve items (use provided products or pre-fill from old contract)
        var resolvedItems = new List<RenewContractProductItemDto>();

        if (request.Products != null && request.Products.Count > 0)
        {
            resolvedItems.AddRange(request.Products);
        }
        else if (oldContract.ContractProducts != null && oldContract.ContractProducts.Count > 0)
        {
            foreach (var cp in oldContract.ContractProducts)
            {
                resolvedItems.Add(new RenewContractProductItemDto
                {
                    ProductID = cp.ProductID,
                    ContractQuantity = cp.ContractQuantity,
                    UnitPrice = cp.UnitPrice
                });
            }
        }
        else if (oldContract.ProductID > 0 && oldContract.TotalQuantity > 0)
        {
            resolvedItems.Add(new RenewContractProductItemDto
            {
                ProductID = oldContract.ProductID,
                ContractQuantity = oldContract.TotalQuantity
            });
        }

        if (resolvedItems.Count == 0)
            throw new InvalidOperationException("At least one product assignment is required for contract renewal.");

        // Validate each item
        foreach (var item in resolvedItems)
        {
            if (item.ProductID <= 0)
                throw new InvalidOperationException("A valid product ID is required.");

            if (item.ContractQuantity <= 0)
                throw new InvalidOperationException("Contract quantity must be greater than zero.");

            var product = await _productRepository.GetByIdAsync(item.ProductID);
            if (product == null)
                throw new InvalidOperationException($"Product ID #{item.ProductID} does not exist.");

            if (!string.Equals(product.Status, "Active", StringComparison.OrdinalIgnoreCase))
                throw new InvalidOperationException($"Product '{product.ProductName}' (ID #{item.ProductID}) is not active.");

            var vendorProduct = await _vendorProductRepository.GetByVendorAndProductAsync(vendorId.Value, item.ProductID);
            if (vendorProduct == null || !string.Equals(vendorProduct.Status, "Active", StringComparison.OrdinalIgnoreCase))
                throw new InvalidOperationException($"Vendor '{vendor.VendorName}' does not actively supply product '{product.ProductName}'.");

            if (!item.UnitPrice.HasValue || item.UnitPrice.Value <= 0)
            {
                item.UnitPrice = vendorProduct.UnitPrice;
            }
        }

        // Check for duplicate products in the renewal request
        if (resolvedItems.GroupBy(x => x.ProductID).Any(g => g.Count() > 1))
        {
            var duplicate = resolvedItems.GroupBy(x => x.ProductID).First(g => g.Count() > 1);
            var p = await _productRepository.GetByIdAsync(duplicate.Key);
            var pName = p?.ProductName ?? $"#{duplicate.Key}";
            throw new InvalidOperationException($"Duplicate assignment for Product '{pName}' in the renewal request.");
        }

        // Duplicate Active Overlapping Contract Rule:
        // Reject overlapping active contracts for the exact (Outlet + Vendor + Product) combination
        var allContracts = await _contractRepository.GetAllAsync();

        foreach (var item in resolvedItems)
        {
            var overlappingContract = allContracts.FirstOrDefault(c =>
                c.ContractID != oldContract.ContractID &&
                c.OutletID == oldContract.OutletID &&
                (c.VendorID == vendorId.Value || c.VendorAllocations.Any(va => va.VendorID == vendorId.Value && string.Equals(va.Status, "Active", StringComparison.OrdinalIgnoreCase))) &&
                string.Equals(c.Status, "Active", StringComparison.OrdinalIgnoreCase) &&
                c.EndDate >= now &&
                c.StartDate < request.EndDate &&
                c.EndDate > request.StartDate &&
                (c.ContractProducts.Any(cp => cp.ProductID == item.ProductID) || (!c.ContractProducts.Any() && c.ProductID == item.ProductID)));

            if (overlappingContract != null)
            {
                var p = await _productRepository.GetByIdAsync(item.ProductID);
                var pName = p?.ProductName ?? $"#{item.ProductID}";
                throw new InvalidOperationException(
                    $"An active contract already exists for Outlet '{outlet.OutletName}', Vendor '{vendor.VendorName}' and Product '{pName}' covering an overlapping validity period ({overlappingContract.StartDate:yyyy-MM-dd} to {overlappingContract.EndDate:yyyy-MM-dd}).");
            }
        }

        var paymentMethod = !string.IsNullOrWhiteSpace(request.PaymentMethod)
            ? request.PaymentMethod
            : (!string.IsNullOrWhiteSpace(oldContract.PaymentMethod) ? oldContract.PaymentMethod : "Net30");

        decimal totalQty = resolvedItems.Sum(x => x.ContractQuantity);
        var firstItem = resolvedItems.First();

        // Create the brand-new Contract record (Old contract is never touched)
        var newContract = new Contract
        {
            QuotationID = null,
            OutletID = oldContract.OutletID,
            VendorID = vendorId.Value,
            ProductID = firstItem.ProductID, // Legacy field
            TotalQuantity = totalQty,        // Legacy field
            UsedQuantity = 0,               // Legacy field: reset to 0
            StartDate = request.StartDate,
            EndDate = request.EndDate,
            PaymentMethod = paymentMethod,
            Status = "Active",
            ContractProducts = resolvedItems.Select(gi => new ContractProduct
            {
                ProductID = gi.ProductID,
                ContractQuantity = gi.ContractQuantity,
                PurchasedQuantity = 0, // Strict reset to 0
                UnitPrice = gi.UnitPrice
            }).ToList(),
            VendorAllocations = new List<ContractVendorAllocation>
            {
                new ContractVendorAllocation
                {
                    VendorID = vendorId.Value,
                    AllocationPercentage = 100m,
                    AllocatedQuantity = totalQty,
                    UsedQuantity = 0, // Reset to 0
                    Status = "Active"
                }
            }
        };

        // Persist new contract via repository
        await _contractRepository.AddAsync(newContract);

        // Cache products for DTO mapping
        var productDict = new Dictionary<int, Product>();
        foreach (var item in resolvedItems)
        {
            var p = await _productRepository.GetByIdAsync(item.ProductID);
            if (p != null) productDict[item.ProductID] = p;
        }

        var firstCp = newContract.ContractProducts.FirstOrDefault();
        var firstP = firstCp != null && productDict.TryGetValue(firstCp.ProductID, out var fp) ? fp : null;

        var createdDto = new ContractDto
        {
            ContractID = newContract.ContractID,
            QuotationID = newContract.QuotationID,
            OutletID = newContract.OutletID,
            OutletName = outlet.OutletName,
            OrganizationID = outlet.OrganizationID,
            OrganizationName = outlet.Organization?.OrganizationName ?? "Organization",
            VendorID = newContract.VendorID,
            VendorName = vendor.VendorName,
            ProductID = firstCp?.ProductID ?? newContract.ProductID,
            ProductName = firstP?.ProductName ?? $"Product #{newContract.ProductID}",
            Unit = firstP?.Unit ?? "Kg",
            TotalQuantity = newContract.ContractProducts.Sum(cp => cp.ContractQuantity),
            UsedQuantity = 0,
            ContractTotalQuantity = newContract.ContractProducts.Sum(cp => cp.ContractQuantity),
            StartDate = newContract.StartDate,
            EndDate = newContract.EndDate,
            PaymentMethod = newContract.PaymentMethod,
            Status = newContract.Status,
            Products = newContract.ContractProducts.Select(cp =>
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
                    PurchasedQuantity = 0,
                    UnitPrice = cp.UnitPrice
                };
            }).ToList(),
            Allocations = newContract.VendorAllocations.Select(a => new ContractVendorAllocationDto
            {
                ContractVendorAllocationID = a.ContractVendorAllocationID,
                VendorID = a.VendorID,
                VendorName = vendor.VendorName,
                AllocationPercentage = a.AllocationPercentage,
                AllocatedQuantity = a.AllocatedQuantity,
                UsedQuantity = 0,
                Status = a.Status
            }).ToList()
        };

        return new RenewContractResponse
        {
            NewContract = createdDto,
            RenewedFromContractID = oldContract.ContractID,
            Success = true,
            Message = $"Contract #{oldContract.ContractID} successfully renewed into new Contract #{newContract.ContractID}."
        };
    }
}
