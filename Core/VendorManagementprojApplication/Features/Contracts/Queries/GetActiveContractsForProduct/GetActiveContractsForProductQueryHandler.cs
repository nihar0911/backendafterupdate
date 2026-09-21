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

namespace VendorManagementprojApplication.Features.Contracts.Queries.GetActiveContractsForProduct;

public class GetActiveContractsForProductQueryHandler : IRequestHandler<GetActiveContractsForProductQuery, GetActiveContractsForProductResponse>
{
    private readonly IContractRepository _contractRepository;
    private readonly IOutletRepository _outletRepository;
    private readonly ICurrentUserService _currentUserService;

    public GetActiveContractsForProductQueryHandler(
        IContractRepository contractRepository,
        IOutletRepository outletRepository,
        ICurrentUserService currentUserService)
    {
        _contractRepository = contractRepository;
        _outletRepository = outletRepository;
        _currentUserService = currentUserService;
    }

    public async Task<GetActiveContractsForProductResponse> Handle(GetActiveContractsForProductQuery request, CancellationToken cancellationToken)
    {
        if (_currentUserService.IsOrganizationManager)
        {
            if (!_currentUserService.OrganizationID.HasValue)
            {
                return new GetActiveContractsForProductResponse { Contracts = new List<ContractDto>() };
            }
            var targetOutlet = await _outletRepository.GetByIdAsync(request.OutletID);
            if (targetOutlet == null || targetOutlet.OrganizationID != _currentUserService.OrganizationID.Value)
            {
                return new GetActiveContractsForProductResponse { Contracts = new List<ContractDto>() };
            }
        }
        else if (_currentUserService.IsOutletManager || _currentUserService.IsPurchaseManager)
        {
            if (_currentUserService.OutletID.HasValue && request.OutletID != _currentUserService.OutletID.Value)
            {
                return new GetActiveContractsForProductResponse { Contracts = new List<ContractDto>() };
            }
        }

        // Phase 2D: Retrieve ALL active contracts matching OutletID + ProductID without filtering on remaining quantity
        var contracts = await _contractRepository.GetActiveContractsByProductAndOutletAsync(request.OutletID, request.ProductID);

        var contractDtos = contracts.Select(c => MapToDto(c, request.ProductID)).ToList();

        return new GetActiveContractsForProductResponse
        {
            Contracts = contractDtos
        };
    }

    private static ContractDto MapToDto(Contract contract, int targetProductId)
    {
        var firstAlloc = contract.VendorAllocations?.FirstOrDefault();
        var targetCp = contract.ContractProducts?.FirstOrDefault(cp => cp.ProductID == targetProductId);

        int resolvedProductId = targetCp?.ProductID ?? contract.ProductID;
        string resolvedProductName = targetCp?.Product?.ProductName ?? contract.Product?.ProductName ?? $"Product #{resolvedProductId}";
        string resolvedUnit = targetCp?.Product?.Unit ?? contract.Product?.Unit ?? "Kg";

        int? vendorId = contract.VendorID ?? firstAlloc?.VendorID;
        string vendorName = contract.Vendor?.VendorName ?? firstAlloc?.Vendor?.VendorName ?? "Vendor";

        // Product-specific quantities for the requested target product
        decimal productContractQty = targetCp != null
            ? targetCp.ContractQuantity
            : ((contract.ContractProducts == null || contract.ContractProducts.Count == 0) ? contract.TotalQuantity : 0m);

        decimal productPurchasedQty = targetCp != null
            ? targetCp.PurchasedQuantity
            : ((contract.ContractProducts == null || contract.ContractProducts.Count == 0) ? contract.UsedQuantity : 0m);

        decimal contractLevelTotalQty = contract.ContractProducts != null && contract.ContractProducts.Count > 0
            ? contract.ContractProducts.Sum(cp => cp.ContractQuantity)
            : contract.TotalQuantity;

        decimal contractLevelUsedQty = contract.ContractProducts != null && contract.ContractProducts.Count > 0
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
            TotalQuantity = productContractQty,
            UsedQuantity = productPurchasedQty,
            ContractQuantity = productContractQty,
            PurchasedQuantity = productPurchasedQty,
            ContractTotalQuantity = contractLevelTotalQty,
            UnitPrice = targetCp?.UnitPrice ?? 0m,
            StartDate = contract.StartDate,
            EndDate = contract.EndDate,
            PaymentMethod = contract.PaymentMethod,
            Status = contract.Status,
            VendorID = vendorId,
            VendorName = vendorName,
            Products = products,
            Allocations = contract.VendorAllocations?.Select(a => new ContractVendorAllocationDto
            {
                ContractVendorAllocationID = a.ContractVendorAllocationID,
                VendorID = a.VendorID,
                VendorName = a.Vendor?.VendorName ?? $"Vendor #{a.VendorID}",
                AllocationPercentage = a.AllocationPercentage,
                AllocatedQuantity = productContractQty,
                UsedQuantity = productPurchasedQty,
                Status = a.Status
            }).ToList() ?? new List<ContractVendorAllocationDto>()
        };
    }
}
