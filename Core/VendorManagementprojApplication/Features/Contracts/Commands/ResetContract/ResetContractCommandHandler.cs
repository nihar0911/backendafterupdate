using MediatR;
using VendorManagementprojApplication.Contracts.Persistence;
using VendorManagementprojApplication.DTOs;

namespace VendorManagementprojApplication.Features.Contracts.Commands.ResetContract;

public class ResetContractCommandHandler : IRequestHandler<ResetContractCommand, ResetContractResponse>
{
    private readonly IContractRepository _contractRepository;

    public ResetContractCommandHandler(IContractRepository contractRepository)
    {
        _contractRepository = contractRepository;
    }

    public async Task<ResetContractResponse> Handle(
        ResetContractCommand request,
        CancellationToken cancellationToken)
    {
        var contract = await _contractRepository.GetByIdAsync(request.ContractID);

        if (contract == null)
            throw new KeyNotFoundException("Contract does not exist.");

        if (!string.Equals(contract.Status, "Reached", StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException(
                "Contract can only be reset after the allocation has been reached.");

        contract.UsedQuantity = 0;
        contract.Status = "Active";

        foreach (var allocation in contract.VendorAllocations)
        {
            allocation.UsedQuantity = 0;
            allocation.Status = "Active";
        }

        var updatedContract = await _contractRepository.UpdateAsync(contract);

        if (updatedContract == null)
            throw new InvalidOperationException("Contract could not be reset.");

        return new ResetContractResponse
        {
            Contract = MapToDto(updatedContract)
        };
    }

    private static ContractDto MapToDto(
        VendorManagementprojDomain.Entities.Contract contract)
    {
        return new ContractDto
        {
            ContractID = contract.ContractID,
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