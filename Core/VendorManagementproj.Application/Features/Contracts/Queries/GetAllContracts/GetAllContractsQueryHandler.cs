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

namespace VendorManagementprojApplication.Features.Contracts.Queries.GetAllContracts;

public class GetAllContractsQueryHandler : IRequestHandler<GetAllContractsQuery, GetAllContractsResponse>
{
    private readonly IContractRepository _contractRepository;
    private readonly IOutletRepository _outletRepository;
    private readonly IQuotationRepository _quotationRepository;
    private readonly ICurrentUserService _currentUserService;

    public GetAllContractsQueryHandler(
        IContractRepository contractRepository,
        IOutletRepository outletRepository,
        IQuotationRepository quotationRepository,
        ICurrentUserService currentUserService)
    {
        _contractRepository = contractRepository;
        _outletRepository = outletRepository;
        _quotationRepository = quotationRepository;
        _currentUserService = currentUserService;
    }

    public async Task<GetAllContractsResponse> Handle(GetAllContractsQuery request, CancellationToken cancellationToken)
    {
        var contracts = await _contractRepository.GetAllAsync();

        if (_currentUserService.IsOrganizationManager)
        {
            if (_currentUserService.OrganizationID.HasValue)
            {
                var orgOutlets = await _outletRepository.GetByOrganizationIdAsync(_currentUserService.OrganizationID.Value);
                var orgOutletIds = orgOutlets.Select(o => o.OutletID).ToHashSet();
                contracts = contracts.Where(c => orgOutletIds.Contains(c.OutletID)).ToList();
            }
            else
            {
                contracts = new List<Contract>();
            }
        }
        else if (_currentUserService.IsOutletManager)
        {
            if (_currentUserService.OutletID.HasValue)
            {
                contracts = contracts.Where(c => c.OutletID == _currentUserService.OutletID.Value).ToList();
            }
            else
            {
                contracts = new List<Contract>();
            }
        }
        else if (_currentUserService.IsPurchaseManager)
        {
            if (_currentUserService.OutletID.HasValue)
            {
                contracts = contracts.Where(c => c.OutletID == _currentUserService.OutletID.Value).ToList();
            }
            else
            {
                contracts = new List<Contract>();
            }
        }

        var allQuotations = await _quotationRepository.GetAllAsync();
        var quotationMap = allQuotations.ToDictionary(q => q.QuotationID);

        var list = contracts.Select(contract =>
        {
            Quotation? quotation = null;
            if (contract.QuotationID.HasValue && quotationMap.TryGetValue(contract.QuotationID.Value, out var qMatched))
            {
                quotation = qMatched;
            }
            else
            {
                var firstAlloc = contract.VendorAllocations?.FirstOrDefault();
                int vendorId = firstAlloc?.VendorID ?? 0;
                quotation = allQuotations
                    .Where(q => q.VendorID == vendorId &&
                                string.Equals(q.Status, "Accepted", StringComparison.OrdinalIgnoreCase) &&
                                q.QuotationItems.Any(qi => qi.ProductID == contract.ProductID && qi.Quantity == contract.TotalQuantity))
                    .OrderByDescending(q => q.QuotationID)
                    .FirstOrDefault();
            }

            return MapToDto(contract, quotation);
        }).ToList();

        return new GetAllContractsResponse
        {
            Contracts = list
        };
    }

    private static ContractDto MapToDto(Contract contract, Quotation? quotation)
    {
        var firstAlloc = contract.VendorAllocations?.FirstOrDefault();
        var qItem = quotation?.QuotationItems?.FirstOrDefault(qi => qi.ProductID == contract.ProductID) ?? quotation?.QuotationItems?.FirstOrDefault();

        decimal unitPrice = qItem?.UnitPrice ?? 0;
        decimal taxAmount = qItem?.TaxAmount ?? 0;
        decimal totalAmount = qItem?.TotalAmount > 0 ? qItem.TotalAmount : (unitPrice * contract.TotalQuantity + taxAmount);

        var firstCp = contract.ContractProducts?.FirstOrDefault();
        int resolvedProductId = firstCp?.ProductID ?? contract.ProductID;
        string resolvedProductName = firstCp?.Product?.ProductName ?? contract.Product?.ProductName ?? $"Product #{resolvedProductId}";
        string resolvedUnit = firstCp?.Product?.Unit ?? contract.Product?.Unit ?? "Kg";

        int? vendorId = contract.VendorID ?? firstAlloc?.VendorID ?? quotation?.VendorID;
        string vendorName = contract.Vendor?.VendorName ?? firstAlloc?.Vendor?.VendorName ?? "Vendor";

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
            QuotationID = contract.QuotationID ?? quotation?.QuotationID,
            OutletID = contract.OutletID,
            OutletName = contract.Outlet?.OutletName ?? $"Outlet #{contract.OutletID}",
            OrganizationID = contract.Outlet?.OrganizationID ?? 0,
            OrganizationName = contract.Outlet?.Organization?.OrganizationName ?? "Organization",
            ProductID = resolvedProductId,
            ProductName = resolvedProductName,
            Unit = resolvedUnit,
            RequestID = quotation?.RequestID,
            VendorID = vendorId,
            VendorName = vendorName,
            TotalQuantity = totalQty,
            UsedQuantity = usedQty,
            ContractTotalQuantity = totalQty,
            UnitPrice = unitPrice,
            TaxAmount = taxAmount,
            TotalAmount = totalAmount,
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
