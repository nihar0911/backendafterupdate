using MediatR;
using VendorManagementprojApplication.Contracts.Persistence;
using VendorManagementprojApplication.Contracts.Services;
using VendorManagementprojApplication.DTOs;
using VendorManagementprojDomain.Entities;

namespace VendorManagementprojApplication.Features.Contracts.Queries.GetContractsByOutlet;

public class GetContractsByOutletQueryHandler : IRequestHandler<GetContractsByOutletQuery, GetContractsByOutletResponse>
{
    private readonly IContractRepository _contractRepository;
    private readonly IOutletRepository _outletRepository;
    private readonly ICurrentUserService _currentUserService;

    public GetContractsByOutletQueryHandler(
        IContractRepository contractRepository,
        IOutletRepository outletRepository,
        ICurrentUserService currentUserService)
    {
        _contractRepository = contractRepository;
        _outletRepository = outletRepository;
        _currentUserService = currentUserService;
    }

    public async Task<GetContractsByOutletResponse> Handle(GetContractsByOutletQuery request, CancellationToken cancellationToken)
    {
        if (_currentUserService.IsOrganizationManager)
        {
            if (!_currentUserService.OrganizationID.HasValue)
            {
                return new GetContractsByOutletResponse { Contracts = new List<ContractDto>() };
            }
            var targetOutlet = await _outletRepository.GetByIdAsync(request.OutletID);
            if (targetOutlet == null || targetOutlet.OrganizationID != _currentUserService.OrganizationID.Value)
            {
                return new GetContractsByOutletResponse { Contracts = new List<ContractDto>() };
            }
        }
        else if (_currentUserService.IsOutletManager)
        {
            if (!_currentUserService.OutletID.HasValue || request.OutletID != _currentUserService.OutletID.Value)
            {
                return new GetContractsByOutletResponse { Contracts = new List<ContractDto>() };
            }
        }

        var allContracts = await _contractRepository.GetAllAsync();
        var outletContracts = allContracts.Where(c => c.OutletID == request.OutletID).Select(MapToDto).ToList();

        return new GetContractsByOutletResponse
        {
            Contracts = outletContracts
        };
    }

    private static ContractDto MapToDto(Contract contract)
    {
        var firstAlloc = contract.VendorAllocations?.FirstOrDefault();
        var firstCp = contract.ContractProducts?.FirstOrDefault();
        int resolvedProductId = firstCp?.ProductID ?? contract.ProductID;
        string resolvedProductName = firstCp?.Product?.ProductName ?? contract.Product?.ProductName ?? $"Product #{resolvedProductId}";
        string resolvedUnit = firstCp?.Product?.Unit ?? contract.Product?.Unit ?? "Kg";

        int? vendorId = contract.VendorID ?? firstAlloc?.VendorID;
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
            QuotationID = contract.QuotationID,
            OutletID = contract.OutletID,
            OutletName = contract.Outlet?.OutletName ?? $"Outlet #{contract.OutletID}",
            OrganizationID = contract.Outlet?.OrganizationID ?? 0,
            OrganizationName = contract.Outlet?.Organization?.OrganizationName ?? "Organization",
            ProductID = resolvedProductId,
            ProductName = resolvedProductName,
            Unit = resolvedUnit,
            TotalQuantity = totalQty,
            UsedQuantity = usedQty,
            ContractTotalQuantity = totalQty,
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
                AllocatedQuantity = a.AllocatedQuantity,
                UsedQuantity = a.UsedQuantity,
                Status = a.Status
            }).ToList() ?? new List<ContractVendorAllocationDto>()
        };
    }
}
