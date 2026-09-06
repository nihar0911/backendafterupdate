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
            Allocations = contract.VendorAllocations.Select(a => new ContractVendorAllocationDto
            {
                ContractVendorAllocationID = a.ContractVendorAllocationID,
                VendorID = a.VendorID,
                AllocationPercentage = a.AllocationPercentage,
                AllocatedQuantity = a.AllocatedQuantity,
                UsedQuantity = a.UsedQuantity,
                Status = a.Status
            }).ToList()
        };
    }
}
