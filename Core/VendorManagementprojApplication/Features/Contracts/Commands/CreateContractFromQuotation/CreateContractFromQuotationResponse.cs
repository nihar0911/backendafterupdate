using VendorManagementprojApplication.DTOs;

namespace VendorManagementprojApplication.Features.Contracts.Commands.CreateContractFromQuotation;

public class CreateContractFromQuotationResponse
{
    public ContractDto? Contract { get; set; }
    public string Message { get; set; } = string.Empty;
}
