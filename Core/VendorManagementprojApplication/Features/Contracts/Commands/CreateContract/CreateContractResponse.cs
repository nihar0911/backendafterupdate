using VendorManagementprojApplication.DTOs;

namespace VendorManagementprojApplication.Features.Contracts.Commands.CreateContract;

public class CreateContractResponse
{
    public ContractDto Contract { get; set; } = null!;
}