using VendorManagementprojApplication.DTOs;

namespace VendorManagementprojApplication.Features.Contracts.Commands.ResetContract;

public class ResetContractResponse
{
    public ContractDto Contract { get; set; } = null!;
}