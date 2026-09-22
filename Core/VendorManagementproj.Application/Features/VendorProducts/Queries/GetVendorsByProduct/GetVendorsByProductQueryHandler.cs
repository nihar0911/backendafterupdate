using MediatR;
using VendorManagementproj.Application.Contracts.Persistence;
using VendorManagementproj.Application.DTOs;

namespace VendorManagementproj.Application.Features.VendorProducts.Queries.GetVendorsByProduct;

public class GetVendorsByProductQueryHandler
    : IRequestHandler<GetVendorsByProductQuery, GetVendorsByProductResponse>
{
    private readonly IVendorProductRepository _vendorProductRepository;
    private readonly IContractRepository _contractRepository;
    private readonly IOutletRepository _outletRepository;

    public GetVendorsByProductQueryHandler(
        IVendorProductRepository vendorProductRepository,
        IContractRepository contractRepository,
        IOutletRepository outletRepository)
    {
        _vendorProductRepository = vendorProductRepository;
        _contractRepository = contractRepository;
        _outletRepository = outletRepository;
    }

    public async Task<GetVendorsByProductResponse> Handle(
        GetVendorsByProductQuery request,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.ProductName))
            throw new InvalidOperationException(
                "Product name is required.");

        var outlet =
            await _outletRepository.GetByIdAsync(request.OutletID);

        if (outlet == null)
            throw new InvalidOperationException(
                "Outlet does not exist.");

        var vendorProducts =
            await _vendorProductRepository
                .GetByProductNameForOutletAsync(
                    request.ProductName,
                    request.OutletID);

        var contracts =
            await _contractRepository.GetAllAsync();

        var result = new List<VendorProductSearchDto>();

        foreach (var vendorProduct in vendorProducts)
        {
            var matchingContract = contracts.FirstOrDefault(c =>
                c.OutletID == request.OutletID &&
                (c.VendorID == vendorProduct.VendorID || c.VendorAllocations.Any(va => va.VendorID == vendorProduct.VendorID && string.Equals(va.Status, "Active", StringComparison.OrdinalIgnoreCase))) &&
                string.Equals(c.Status, "Active", StringComparison.OrdinalIgnoreCase) &&
                c.StartDate <= DateTime.Now &&
                c.EndDate >= DateTime.Now &&
                (c.ContractProducts.Any(cp => cp.ProductID == vendorProduct.ProductID) || (!c.ContractProducts.Any() && c.ProductID == vendorProduct.ProductID)));

            bool hasActiveContract = matchingContract != null;
            var cp = matchingContract?.ContractProducts.FirstOrDefault(p => p.ProductID == vendorProduct.ProductID);
            var allocation = matchingContract?.VendorAllocations.FirstOrDefault(va => va.VendorID == vendorProduct.VendorID);

            decimal allocatedQty = cp?.ContractQuantity ?? (matchingContract != null && !matchingContract.ContractProducts.Any() ? (allocation?.AllocatedQuantity ?? matchingContract.TotalQuantity) : 0m);
            decimal usedQty = cp?.PurchasedQuantity ?? (matchingContract != null && !matchingContract.ContractProducts.Any() ? (allocation?.UsedQuantity ?? matchingContract.UsedQuantity) : 0m);
            decimal remainingQty = Math.Max(0m, allocatedQty - usedQty);

            result.Add(new VendorProductSearchDto
            {
                VendorProductID = vendorProduct.VendorProductID,
                VendorID = vendorProduct.VendorID,
                ProductID = vendorProduct.ProductID,
                UnitPrice = cp?.UnitPrice ?? vendorProduct.UnitPrice,
                EstimatedDeliveryDays = vendorProduct.EstimatedDeliveryDays,
                Status = vendorProduct.Status,
                HasActiveContract = hasActiveContract,
                ContractID = matchingContract?.ContractID,
                ContractQuantity = hasActiveContract ? (cp?.ContractQuantity ?? (!matchingContract!.ContractProducts.Any() ? matchingContract.TotalQuantity : null)) : null,
                PurchasedQuantity = hasActiveContract ? (cp?.PurchasedQuantity ?? (!matchingContract!.ContractProducts.Any() ? matchingContract.UsedQuantity : null)) : null,
                AllocationPercentage = hasActiveContract ? (allocation?.AllocationPercentage ?? 100m) : 0m,
                AllocatedQuantity = allocatedQty,
                UsedQuantity = usedQty,
                RemainingQuantity = remainingQty
            });
        }

        var orderedResult = result
            .OrderBy(x => x.UnitPrice)
            .ToList();

        return new GetVendorsByProductResponse
        {
            Vendors = orderedResult
        };
    }
}