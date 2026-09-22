using VendorManagementproj.Application.DTOs;

namespace VendorManagementproj.Application.Features.Contracts.Commands.ResetContract;

public class ResetContractResponse
{
    public ContractDto Contract { get; set; } = null!;
}