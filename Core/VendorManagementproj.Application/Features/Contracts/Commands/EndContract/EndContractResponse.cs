using VendorManagementproj.Application.DTOs;

namespace VendorManagementproj.Application.Features.Contracts.Commands.EndContract;

public class EndContractResponse
{
    public ContractDto? Contract { get; set; }
    public bool Success { get; set; } = true;
    public string Message { get; set; } = "Contract has been successfully ended.";
}
