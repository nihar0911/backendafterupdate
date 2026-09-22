using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using VendorManagementprojApplication.Contracts.Persistence;
using VendorManagementprojApplication.Contracts.Services;
using VendorManagementprojApplication.DTOs;
using VendorManagementprojDomain.Entities;

namespace VendorManagementprojApplication.Features.Contracts.Commands.CreateContractFromQuotation;

public class CreateContractFromQuotationCommandHandler : IRequestHandler<CreateContractFromQuotationCommand, CreateContractFromQuotationResponse>
{
    private readonly IContractRepository _contractRepository;
    private readonly IQuotationRepository _quotationRepository;
    private readonly IPurchaseRequestRepository _purchaseRequestRepository;
    private readonly IOutletRepository _outletRepository;
    private readonly IProductRepository _productRepository;
    private readonly IVendorRepository _vendorRepository;
    private readonly IVendorProductRepository _vendorProductRepository;
    private readonly ICurrentUserService _currentUserService;

    public CreateContractFromQuotationCommandHandler(
        IContractRepository contractRepository,
        IQuotationRepository quotationRepository,
        IPurchaseRequestRepository purchaseRequestRepository,
        IOutletRepository outletRepository,
        IProductRepository productRepository,
        IVendorRepository vendorRepository,
        IVendorProductRepository vendorProductRepository,
        ICurrentUserService currentUserService)
    {
        _contractRepository = contractRepository;
        _quotationRepository = quotationRepository;
        _purchaseRequestRepository = purchaseRequestRepository;
        _outletRepository = outletRepository;
        _productRepository = productRepository;
        _vendorRepository = vendorRepository;
        _vendorProductRepository = vendorProductRepository;
        _currentUserService = currentUserService;
    }

    public async Task<CreateContractFromQuotationResponse> Handle(CreateContractFromQuotationCommand request, CancellationToken cancellationToken)
    {
        var quotation = await _quotationRepository.GetByIdAsync(request.QuotationID);
        if (quotation == null)
            throw new KeyNotFoundException($"Quotation with ID {request.QuotationID} was not found.");

        if (!string.Equals(quotation.Status, "Accepted", StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException("Contract can only be created from an ACCEPTED quotation.");

        var purchaseRequest = await _purchaseRequestRepository.GetByIdAsync(quotation.RequestID);
        if (purchaseRequest == null)
            throw new KeyNotFoundException($"Purchase request with ID {quotation.RequestID} was not found.");

        if (!string.Equals(purchaseRequest.Status, "Approved", StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException("Purchase request must be Approved before creating a contract.");

        var targetOutlet = await _outletRepository.GetByIdAsync(purchaseRequest.OutletID);
        if (targetOutlet == null)
            throw new InvalidOperationException("Target outlet does not exist.");

        if (_currentUserService.IsOrganizationManager)
        {
            if (!_currentUserService.OrganizationID.HasValue || targetOutlet.OrganizationID != _currentUserService.OrganizationID.Value)
            {
                throw new UnauthorizedAccessException("You cannot create a contract for an outlet outside your organization.");
            }
        }
        else if (_currentUserService.IsOutletManager)
        {
            throw new UnauthorizedAccessException("Outlet Managers cannot create contracts.");
        }
        else if (!_currentUserService.IsAdmin)
        {
            throw new UnauthorizedAccessException("You are not authorized to create contracts.");
        }

        var qItem = quotation.QuotationItems?.FirstOrDefault();
        var prItem = purchaseRequest.Items?.FirstOrDefault();

        int productId = qItem?.ProductID ?? prItem?.ProductID ?? 0;
        decimal quantity = qItem?.Quantity ?? prItem?.Quantity ?? 0;

        if (productId <= 0)
            throw new InvalidOperationException("Quotation does not specify a valid Product.");

        if (quantity <= 0)
            throw new InvalidOperationException("Quotation quantity must be greater than zero.");

        var product = await _productRepository.GetByIdAsync(productId);
        var primaryVendor = await _vendorRepository.GetByIdAsync(quotation.VendorID);

        // Prevent duplicate active contract based on QuotationID == quotation.QuotationID AND Status == "Active"
        var existingContracts = await _contractRepository.GetAllAsync();
        var duplicateContract = existingContracts.FirstOrDefault(c =>
            c.QuotationID == quotation.QuotationID &&
            string.Equals(c.Status, "Active", StringComparison.OrdinalIgnoreCase));

        if (duplicateContract != null)
        {
            var existingDto = MapToEnrichedDto(duplicateContract, targetOutlet, product, primaryVendor, quotation, purchaseRequest);
            return new CreateContractFromQuotationResponse
            {
                Contract = existingDto,
                Message = $"Contract #{duplicateContract.ContractID} already exists for this accepted quotation."
            };
        }

        DateTime startDate = DateTime.Now;
        DateTime endDate = quotation.ValidUntil > startDate ? quotation.ValidUntil : startDate.AddYears(1);

        var contract = new Contract
        {
            QuotationID = quotation.QuotationID,
            OutletID = purchaseRequest.OutletID,
            ProductID = productId,
            TotalQuantity = quantity,
            UsedQuantity = 0,
            StartDate = startDate,
            EndDate = endDate,
            PaymentMethod = "Bank",
            Status = "Active",
            VendorAllocations = new List<ContractVendorAllocation>()
        };

        // Determine allocations: either configured by user or default 100% to accepted quotation vendor
        if (request.Allocations != null && request.Allocations.Count > 0)
        {
            if (request.Allocations.Any(x => x.AllocationPercentage < 0 || x.AllocationPercentage > 100))
                throw new InvalidOperationException("Allocation percentage must be between 0% and 100%.");

            var positiveAllocations = request.Allocations.Where(x => x.AllocationPercentage > 0).ToList();

            if (positiveAllocations.Count == 0)
                throw new InvalidOperationException("At least one vendor must have a positive allocation percentage.");

            if (positiveAllocations.GroupBy(x => x.VendorID).Any(g => g.Count() > 1))
                throw new InvalidOperationException("A vendor cannot be allocated more than once.");

            var totalPercentage = positiveAllocations.Sum(x => x.AllocationPercentage);
            if (totalPercentage != 100m)
                throw new InvalidOperationException($"Vendor allocation percentages must total exactly 100%. Current total: {totalPercentage:0.##}%.");

            foreach (var alloc in positiveAllocations)
            {
                var vendor = await _vendorRepository.GetByIdAsync(alloc.VendorID);
                if (vendor == null)
                    throw new InvalidOperationException($"Vendor ID #{alloc.VendorID} does not exist.");

                if (!string.Equals(vendor.Status, "Active", StringComparison.OrdinalIgnoreCase))
                    throw new InvalidOperationException($"Vendor '{vendor.VendorName}' (#{alloc.VendorID}) is not active.");

                var vendorProduct = await _vendorProductRepository.GetByVendorAndProductAsync(alloc.VendorID, productId);
                if (vendorProduct == null || !string.Equals(vendorProduct.Status, "Active", StringComparison.OrdinalIgnoreCase))
                    throw new InvalidOperationException($"Vendor '{vendor.VendorName}' (#{alloc.VendorID}) does not have an active catalog mapping for {product?.ProductName ?? $"Product #{productId}"}.");

                var allocatedQuantity = Math.Round(quantity * alloc.AllocationPercentage / 100m, 2, MidpointRounding.AwayFromZero);

                contract.VendorAllocations.Add(new ContractVendorAllocation
                {
                    VendorID = alloc.VendorID,
                    AllocationPercentage = alloc.AllocationPercentage,
                    AllocatedQuantity = allocatedQuantity,
                    UsedQuantity = 0,
                    Status = "Active"
                });
            }

            // Adjust any fractional rounding difference to guarantee exact total
            var totalAllocated = contract.VendorAllocations.Sum(a => a.AllocatedQuantity);
            var delta = quantity - totalAllocated;
            if (delta != 0 && contract.VendorAllocations.Count > 0)
            {
                var largest = contract.VendorAllocations.OrderByDescending(a => a.AllocatedQuantity).First();
                largest.AllocatedQuantity += delta;
            }
        }
        else
        {
            // Default single-vendor 100% allocation
            contract.VendorAllocations.Add(new ContractVendorAllocation
            {
                VendorID = quotation.VendorID,
                AllocationPercentage = 100,
                AllocatedQuantity = quantity,
                UsedQuantity = 0,
                Status = "Active"
            });
        }

        var createdContract = await _contractRepository.AddAsync(contract);

        var resultDto = MapToEnrichedDto(createdContract, targetOutlet, product, primaryVendor, quotation, purchaseRequest);

        return new CreateContractFromQuotationResponse
        {
            Contract = resultDto,
            Message = "Contract created successfully!"
        };
    }

    private static ContractDto MapToEnrichedDto(
        Contract contract,
        Outlet? outlet,
        Product? product,
        Vendor? vendor,
        Quotation? quotation,
        PurchaseRequest? purchaseRequest)
    {
        var firstQItem = quotation?.QuotationItems?.FirstOrDefault();
        decimal unitPrice = firstQItem?.UnitPrice ?? 0;
        decimal taxAmount = firstQItem?.TaxAmount ?? 0;
        decimal totalAmount = firstQItem?.TotalAmount > 0 ? firstQItem.TotalAmount : (unitPrice * contract.TotalQuantity + taxAmount);

        return new ContractDto
        {
            ContractID = contract.ContractID,
            QuotationID = contract.QuotationID ?? quotation?.QuotationID,
            OutletID = contract.OutletID,
            OutletName = outlet?.OutletName ?? $"Outlet #{contract.OutletID}",
            OrganizationID = outlet?.OrganizationID ?? 0,
            OrganizationName = outlet?.Organization?.OrganizationName ?? "Organization",
            ProductID = contract.ProductID,
            ProductName = product?.ProductName ?? (purchaseRequest?.Items?.FirstOrDefault()?.Product?.ProductName ?? $"Product #{contract.ProductID}"),
            Unit = purchaseRequest?.Items?.FirstOrDefault()?.Unit ?? product?.Unit ?? "Kg",
            RequestID = purchaseRequest?.RequestID ?? quotation?.RequestID,
            VendorID = vendor?.VendorID ?? contract.VendorAllocations.FirstOrDefault()?.VendorID,
            VendorName = vendor?.VendorName ?? contract.VendorAllocations.FirstOrDefault()?.Vendor?.VendorName ?? "Vendor",
            TotalQuantity = contract.TotalQuantity,
            UsedQuantity = contract.UsedQuantity,
            UnitPrice = unitPrice,
            TaxAmount = taxAmount,
            TotalAmount = totalAmount,
            StartDate = contract.StartDate,
            EndDate = contract.EndDate,
            PaymentMethod = contract.PaymentMethod,
            Status = contract.Status,
            Allocations = contract.VendorAllocations.Select(a => new ContractVendorAllocationDto
            {
                ContractVendorAllocationID = a.ContractVendorAllocationID,
                VendorID = a.VendorID,
                VendorName = a.Vendor?.VendorName ?? vendor?.VendorName ?? $"Vendor #{a.VendorID}",
                AllocationPercentage = a.AllocationPercentage,
                AllocatedQuantity = a.AllocatedQuantity,
                UsedQuantity = a.UsedQuantity,
                Status = a.Status
            }).ToList()
        };
    }
}
