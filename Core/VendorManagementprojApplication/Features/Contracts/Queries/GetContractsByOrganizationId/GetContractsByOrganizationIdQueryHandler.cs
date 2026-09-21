using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using VendorManagementprojApplication.Contracts.Persistence;
using VendorManagementprojApplication.Contracts.Services;
using VendorManagementprojApplication.DTOs;

namespace VendorManagementprojApplication.Features.Contracts.Queries.GetContractsByOrganizationId;

public class GetContractsByOrganizationIdQueryHandler : IRequestHandler<GetContractsByOrganizationIdQuery, GetContractsByOrganizationIdResponse>
{
    private readonly IContractRepository _contractRepository;
    private readonly IOutletRepository _outletRepository;
    private readonly ICurrentUserService _currentUserService;

    public GetContractsByOrganizationIdQueryHandler(
        IContractRepository contractRepository,
        IOutletRepository outletRepository,
        ICurrentUserService currentUserService)
    {
        _contractRepository = contractRepository;
        _outletRepository = outletRepository;
        _currentUserService = currentUserService;
    }

    public async Task<GetContractsByOrganizationIdResponse> Handle(GetContractsByOrganizationIdQuery request, CancellationToken cancellationToken)
    {
        if (_currentUserService.IsOrganizationManager && _currentUserService.OrganizationID.HasValue)
        {
            if (request.OrganizationID != _currentUserService.OrganizationID.Value)
            {
                throw new System.UnauthorizedAccessException("You cannot view contracts for another organization.");
            }
        }

        var orgOutlets = await _outletRepository.GetByOrganizationIdAsync(request.OrganizationID);
        var orgOutletIds = orgOutlets.Select(o => o.OutletID).ToHashSet();

        var allContracts = await _contractRepository.GetAllAsync();
        var orgContracts = allContracts.Where(c => orgOutletIds.Contains(c.OutletID)).ToList();

        var dtos = orgContracts.Select(c =>
        {
            var firstCp = c.ContractProducts?.FirstOrDefault();
            int resolvedProductId = firstCp?.ProductID ?? c.ProductID;
            string resolvedProductName = firstCp?.Product?.ProductName ?? c.Product?.ProductName ?? $"Product #{resolvedProductId}";
            string resolvedUnit = firstCp?.Product?.Unit ?? c.Product?.Unit ?? "Kg";

            int? vendorId = c.VendorID ?? c.VendorAllocations.FirstOrDefault()?.VendorID;
            string vendorName = c.Vendor?.VendorName ?? c.VendorAllocations.FirstOrDefault()?.Vendor?.VendorName ?? "Vendor";

            decimal totalQty = c.ContractProducts != null && c.ContractProducts.Count > 0
                ? c.ContractProducts.Sum(cp => cp.ContractQuantity)
                : c.TotalQuantity;

            decimal usedQty = c.ContractProducts != null && c.ContractProducts.Count > 0
                ? c.ContractProducts.Sum(cp => cp.PurchasedQuantity)
                : c.UsedQuantity;

            var products = c.ContractProducts?.Select(cp => new ContractProductDto
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
                ContractID = c.ContractID,
                OutletID = c.OutletID,
                OutletName = c.Outlet?.OutletName ?? $"Outlet #{c.OutletID}",
                OrganizationID = c.Outlet?.OrganizationID ?? request.OrganizationID,
                OrganizationName = c.Outlet?.Organization?.OrganizationName ?? "Organization",
                ProductID = resolvedProductId,
                ProductName = resolvedProductName,
                Unit = resolvedUnit,
                TotalQuantity = totalQty,
                UsedQuantity = usedQty,
                ContractTotalQuantity = totalQty,
                StartDate = c.StartDate,
                EndDate = c.EndDate,
                PaymentMethod = c.PaymentMethod,
                Status = c.Status,
                VendorID = vendorId,
                VendorName = vendorName,
                Products = products,
                Allocations = c.VendorAllocations.Select(a => new ContractVendorAllocationDto
                {
                    ContractVendorAllocationID = a.ContractVendorAllocationID,
                    VendorID = a.VendorID,
                    VendorName = a.Vendor?.VendorName ?? $"Vendor #{a.VendorID}",
                    AllocationPercentage = a.AllocationPercentage,
                    AllocatedQuantity = a.AllocatedQuantity,
                    UsedQuantity = a.UsedQuantity,
                    Status = a.Status
                }).ToList()
            };
        }).ToList();

        return new GetContractsByOrganizationIdResponse
        {
            Contracts = dtos
        };
    }
}
