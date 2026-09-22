using MediatR;
using VendorManagementprojApplication.Contracts.Persistence;
using VendorManagementprojApplication.DTOs;

namespace VendorManagementprojApplication.Features.Contracts.Commands.UpdateContract;

public class UpdateContractCommandHandler
    : IRequestHandler<UpdateContractCommand, UpdateContractResponse>
{
    private readonly IContractRepository _contractRepository;

    public UpdateContractCommandHandler(
        IContractRepository contractRepository)
    {
        _contractRepository = contractRepository;
    }

    public async Task<UpdateContractResponse> Handle(
        UpdateContractCommand request,
        CancellationToken cancellationToken)
    {
        if (request.ContractID <= 0)
            throw new InvalidOperationException(
                "A valid contract is required.");

        if (request.EndDate <= request.StartDate)
            throw new InvalidOperationException(
                "End date must be after start date.");

        if (!string.Equals(
                request.PaymentMethod,
                "Cash",
                StringComparison.OrdinalIgnoreCase) &&
            !string.Equals(
                request.PaymentMethod,
                "Card",
                StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException(
                "Payment method must be Cash or Card.");
        }

        var contract =
            await _contractRepository.GetByIdAsync(
                request.ContractID);

        if (contract == null)
            throw new KeyNotFoundException(
                "Contract does not exist.");

        if (string.Equals(
                contract.Status,
                "Reached",
                StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException(
                "A reached contract cannot be updated.");
        }

        contract.StartDate =
            request.StartDate;

        contract.EndDate =
            request.EndDate;

        contract.PaymentMethod =
            request.PaymentMethod;

        var updatedContract =
            await _contractRepository.UpdateAsync(
                contract);

        if (updatedContract == null)
            throw new InvalidOperationException(
                "Contract could not be updated.");

        return new UpdateContractResponse
        {
            Contract =
                MapToDto(updatedContract)
        };
    }

    private static ContractDto MapToDto(
        VendorManagementprojDomain.Entities.Contract contract)
    {
        return new ContractDto
        {
            ContractID =
                contract.ContractID,

            OutletID =
                contract.OutletID,

            ProductID =
                contract.ProductID,

            TotalQuantity =
                contract.TotalQuantity,

            UsedQuantity =
                contract.UsedQuantity,

            StartDate =
                contract.StartDate,

            EndDate =
                contract.EndDate,

            PaymentMethod =
                contract.PaymentMethod,

            Status =
                contract.Status,

            Allocations =
                contract.VendorAllocations
                    .Select(allocation =>
                        new ContractVendorAllocationDto
                        {
                            ContractVendorAllocationID =
                                allocation.ContractVendorAllocationID,

                            VendorID =
                                allocation.VendorID,

                            AllocationPercentage =
                                allocation.AllocationPercentage,

                            AllocatedQuantity =
                                allocation.AllocatedQuantity,

                            UsedQuantity =
                                allocation.UsedQuantity,

                            Status =
                                allocation.Status
                        })
                    .ToList()
        };
    }
}