using VendorManagementproj.Application.DTOs;

namespace VendorManagementproj.Application.Features.Contracts.Commands.UpdateContract;

public class UpdateContractResponse
{
    public ContractDto Contract { get; set; } = null!;
}