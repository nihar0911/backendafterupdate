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

namespace VendorManagementprojApplication.Features.Contracts.Commands.CreateContract;

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
        if (request.TotalQuantity <= 0)
            throw new InvalidOperationException("Total quantity must be greater than zero.");

        if (request.EndDate <= request.StartDate)
            throw new InvalidOperationException("End date must be after start date.");

        if (request.Allocations == null || request.Allocations.Count == 0)
            throw new InvalidOperationException("At least one vendor allocation is required.");

        if (request.Allocations.Any(x => x.AllocationPercentage < 0 || x.AllocationPercentage > 100))
            throw new InvalidOperationException("Allocation percentage must be between 0% and 100%.");

        var positiveAllocations = request.Allocations.Where(x => x.AllocationPercentage > 0).ToList();

        if (positiveAllocations.Count == 0)
            throw new InvalidOperationException("At least one vendor must have a positive allocation percentage.");

        if (positiveAllocations
            .GroupBy(x => x.VendorID)
            .Any(g => g.Count() > 1))
            throw new InvalidOperationException("A vendor cannot be allocated more than once.");

        var totalPercentage = positiveAllocations.Sum(x => x.AllocationPercentage);

        if (totalPercentage != 100m)
            throw new InvalidOperationException($"Vendor allocation percentages must total exactly 100%. Current total: {totalPercentage:0.##}%.");

        var targetOutlet = await _outletRepository.GetByIdAsync(request.OutletID);
        if (targetOutlet == null)
            throw new InvalidOperationException("Target outlet does not exist.");

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

        var product = await _productRepository.GetByIdAsync(request.ProductID);

        if (product == null)
            throw new InvalidOperationException("Product does not exist.");

        var paymentMethod = !string.IsNullOrWhiteSpace(request.PaymentMethod) ? request.PaymentMethod : "Net30";

        var contract = new Contract
        {
            QuotationID = request.QuotationID,
            OutletID = request.OutletID,
            ProductID = request.ProductID,
            TotalQuantity = request.TotalQuantity,
            UsedQuantity = 0,
            StartDate = request.StartDate,
            EndDate = request.EndDate,
            PaymentMethod = paymentMethod,
            Status = "Active",
            VendorAllocations = new List<ContractVendorAllocation>()
        };

        foreach (var allocation in positiveAllocations)
        {
            var vendor = await _vendorRepository.GetByIdAsync(allocation.VendorID);

            if (vendor == null)
                throw new InvalidOperationException(
                    $"VendorID {allocation.VendorID} does not exist.");

            if (!string.Equals(vendor.Status, "Active", StringComparison.OrdinalIgnoreCase))
                throw new InvalidOperationException(
                    $"VendorID {allocation.VendorID} is not active.");

            var vendorProduct = await _vendorProductRepository
                .GetByVendorAndProductAsync(allocation.VendorID, request.ProductID);

            if (vendorProduct == null)
                throw new InvalidOperationException(
                    $"VendorID {allocation.VendorID} does not supply ProductID {request.ProductID}.");

            if (!string.Equals(vendorProduct.Status, "Active", StringComparison.OrdinalIgnoreCase))
                throw new InvalidOperationException(
                    $"ProductID {request.ProductID} is not active for VendorID {allocation.VendorID}.");

            var allocatedQuantity = Math.Round(request.TotalQuantity * allocation.AllocationPercentage / 100m, 2, MidpointRounding.AwayFromZero);

            contract.VendorAllocations.Add(new ContractVendorAllocation
            {
                VendorID = allocation.VendorID,
                AllocationPercentage = allocation.AllocationPercentage,
                AllocatedQuantity = allocatedQuantity,
                UsedQuantity = 0,
                Status = "Active"
            });
        }

        // Adjust for any small fractional rounding delta to guarantee exact total
        var totalAllocated = contract.VendorAllocations.Sum(a => a.AllocatedQuantity);
        var delta = request.TotalQuantity - totalAllocated;
        if (delta != 0 && contract.VendorAllocations.Count > 0)
        {
            var largest = contract.VendorAllocations.OrderByDescending(a => a.AllocatedQuantity).First();
            largest.AllocatedQuantity += delta;
        }

        var createdContract = await _contractRepository.AddAsync(contract);

        return new CreateContractResponse
        {
            Contract = MapToDto(createdContract)
        };
    }

    private static ContractDto MapToDto(Contract contract)
    {
        return new ContractDto
        {
            ContractID = contract.ContractID,
            QuotationID = contract.QuotationID,
            OutletID = contract.OutletID,
            ProductID = contract.ProductID,
            TotalQuantity = contract.TotalQuantity,
            UsedQuantity = contract.UsedQuantity,
            StartDate = contract.StartDate,
            EndDate = contract.EndDate,
            PaymentMethod = contract.PaymentMethod,
            Status = contract.Status,
            Allocations = contract.VendorAllocations.Select(allocation => new ContractVendorAllocationDto
            {
                ContractVendorAllocationID = allocation.ContractVendorAllocationID,
                VendorID = allocation.VendorID,
                AllocationPercentage = allocation.AllocationPercentage,
                AllocatedQuantity = allocation.AllocatedQuantity,
                UsedQuantity = allocation.UsedQuantity,
                Status = allocation.Status
            }).ToList()
        };
    }
}
