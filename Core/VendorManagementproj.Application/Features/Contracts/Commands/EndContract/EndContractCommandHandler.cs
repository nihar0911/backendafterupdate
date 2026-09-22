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

namespace VendorManagementproj.Application.Features.Contracts.Commands.EndContract;

public class EndContractCommandHandler : IRequestHandler<EndContractCommand, EndContractResponse>
{
    private readonly IContractRepository _contractRepository;
    private readonly IOutletRepository _outletRepository;
    private readonly ICurrentUserService _currentUserService;

    public EndContractCommandHandler(
        IContractRepository contractRepository,
        IOutletRepository outletRepository,
        ICurrentUserService currentUserService)
    {
        _contractRepository = contractRepository;
        _outletRepository = outletRepository;
        _currentUserService = currentUserService;
    }

    public async Task<EndContractResponse> Handle(
        EndContractCommand request,
        CancellationToken cancellationToken)
    {
        if (request.ContractID <= 0)
            throw new InvalidOperationException("A valid contract ID is required.");

        var contract = await _contractRepository.GetByIdAsync(request.ContractID);
        if (contract == null)
            throw new KeyNotFoundException($"Contract #{request.ContractID} does not exist.");

        // Authorization checks
        if (!_currentUserService.IsAdmin && !_currentUserService.IsOrganizationManager)
        {
            throw new UnauthorizedAccessException("Only an Organization Manager or Admin is authorized to end contracts.");
        }

        if (_currentUserService.IsOrganizationManager)
        {
            if (!_currentUserService.OrganizationID.HasValue)
            {
                throw new UnauthorizedAccessException("Organization context is missing.");
            }

            var targetOutlet = contract.Outlet ?? await _outletRepository.GetByIdAsync(contract.OutletID);
            if (targetOutlet == null || targetOutlet.OrganizationID != _currentUserService.OrganizationID.Value)
            {
                throw new UnauthorizedAccessException("You cannot end contracts for an outlet outside your organization.");
            }
        }

        // Prevent ending an already ended or inactive contract
        if (string.Equals(contract.Status, "Ended", StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("Contract has already been ended.");
        }

        // Update status to Ended while strictly preserving all history, quantities, and products
        contract.Status = "Ended";

        if (contract.VendorAllocations != null)
        {
            foreach (var alloc in contract.VendorAllocations)
            {
                alloc.Status = "Ended";
            }
        }

        var updated = await _contractRepository.UpdateAsync(contract);
        if (updated == null)
        {
            throw new InvalidOperationException("Failed to update contract status.");
        }

        return new EndContractResponse
        {
            Contract = MapToDto(updated),
            Success = true,
            Message = "Contract has been successfully ended."
        };
    }

    private static ContractDto MapToDto(Contract contract)
    {
        var firstCp = contract.ContractProducts?.FirstOrDefault();
        int resolvedProductId = firstCp?.ProductID ?? contract.ProductID;
        string resolvedProductName = firstCp?.Product?.ProductName ?? contract.Product?.ProductName ?? $"Product #{resolvedProductId}";
        string resolvedUnit = firstCp?.Product?.Unit ?? contract.Product?.Unit ?? "Kg";

        var firstAlloc = contract.VendorAllocations?.FirstOrDefault();
        int? vendorId = contract.VendorID ?? firstAlloc?.VendorID;
        string vendorName = contract.Vendor?.VendorName ?? firstAlloc?.Vendor?.VendorName ?? $"Vendor #{vendorId}";

        decimal totalQty = contract.ContractProducts != null && contract.ContractProducts.Count > 0
            ? contract.ContractProducts.Sum(cp => cp.ContractQuantity)
            : contract.TotalQuantity;

        decimal usedQty = contract.ContractProducts != null && contract.ContractProducts.Count > 0
            ? contract.ContractProducts.Sum(cp => cp.PurchasedQuantity)
            : contract.UsedQuantity;

        var products = contract.ContractProducts?.Select(cp => new ContractProductDto
        {
            ContractProductID = cp.ContractProductID,
            ContractID = cp.ContractID,
            ProductID = cp.ProductID,
            ProductName = cp.Product?.ProductName ?? $"Product #{cp.ProductID}",
            Unit = cp.Product?.Unit ?? "Kg",
            ContractQuantity = cp.ContractQuantity,
            PurchasedQuantity = cp.PurchasedQuantity,
            UnitPrice = cp.UnitPrice
        }).ToList() ?? new List<ContractProductDto>();

        return new ContractDto
        {
            ContractID = contract.ContractID,
            QuotationID = contract.QuotationID,
            OutletID = contract.OutletID,
            OutletName = contract.Outlet?.OutletName ?? $"Outlet #{contract.OutletID}",
            OrganizationID = contract.Outlet?.OrganizationID ?? 0,
            OrganizationName = contract.Outlet?.Organization?.OrganizationName ?? "Organization",
            ProductID = resolvedProductId,
            ProductName = resolvedProductName,
            Unit = resolvedUnit,
            VendorID = vendorId,
            VendorName = vendorName,
            TotalQuantity = totalQty,
            UsedQuantity = usedQty,
            ContractTotalQuantity = totalQty,
            StartDate = contract.StartDate,
            EndDate = contract.EndDate,
            PaymentMethod = contract.PaymentMethod,
            Status = contract.Status,
            Products = products,
            Allocations = contract.VendorAllocations?.Select(a => new ContractVendorAllocationDto
            {
                ContractVendorAllocationID = a.ContractVendorAllocationID,
                VendorID = a.VendorID,
                VendorName = a.Vendor?.VendorName ?? $"Vendor #{a.VendorID}",
                AllocationPercentage = a.AllocationPercentage,
                AllocatedQuantity = a.AllocatedQuantity,
                UsedQuantity = a.UsedQuantity,
                Status = a.Status
            }).ToList() ?? new List<ContractVendorAllocationDto>()
        };
    }
}
