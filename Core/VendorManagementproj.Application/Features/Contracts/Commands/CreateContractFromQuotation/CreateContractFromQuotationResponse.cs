using VendorManagementproj.Application.DTOs;

namespace VendorManagementproj.Application.Features.Contracts.Commands.CreateContractFromQuotation;

public class CreateContractFromQuotationResponse
{
    public ContractDto? Contract { get; set; }
    public string Message { get; set; } = string.Empty;
}
