using VendorManagementprojApplication.DTOs;

namespace VendorManagementprojApplication.Features.Contracts.Commands.UpdateContract;

public class UpdateContractResponse
{
    public ContractDto Contract { get; set; } = null!;
}