using MediatR;
using VendorManagementprojApplication.Contracts.Persistence;
using VendorManagementprojApplication.DTOs;

namespace VendorManagementprojApplication.Features.VendorProducts.Queries.GetVendorsByProduct;

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
            var allocation = contracts
                .Where(contract =>
                    contract.OutletID == request.OutletID &&
                    contract.ProductID == vendorProduct.ProductID &&
                    string.Equals(
                        contract.Status,
                        "Active",
                        StringComparison.OrdinalIgnoreCase) &&
                    contract.StartDate <= DateTime.Now &&
                    contract.EndDate >= DateTime.Now)
                .SelectMany(contract => contract.VendorAllocations)
                .FirstOrDefault(allocation =>
                    allocation.VendorID == vendorProduct.VendorID &&
                    string.Equals(
                        allocation.Status,
                        "Active",
                        StringComparison.OrdinalIgnoreCase));

            bool hasActiveContract = allocation != null;

            result.Add(new VendorProductSearchDto
            {
                VendorProductID =
                    vendorProduct.VendorProductID,

                VendorID =
                    vendorProduct.VendorID,

                ProductID =
                    vendorProduct.ProductID,

                UnitPrice =
                    vendorProduct.UnitPrice,

                EstimatedDeliveryDays =
                    vendorProduct.EstimatedDeliveryDays,

                Status =
                    vendorProduct.Status,

                HasActiveContract =
                    hasActiveContract,

                AllocationPercentage =
                    hasActiveContract ? allocation!.AllocationPercentage : 0m,

                AllocatedQuantity =
                    hasActiveContract ? allocation!.AllocatedQuantity : 0m,

                UsedQuantity =
                    hasActiveContract ? allocation!.UsedQuantity : 0m,

                RemainingQuantity =
                    hasActiveContract ? (allocation!.AllocatedQuantity - allocation.UsedQuantity) : 0m
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