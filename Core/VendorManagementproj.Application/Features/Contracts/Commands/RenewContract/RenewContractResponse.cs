using VendorManagementprojApplication.DTOs;

namespace VendorManagementprojApplication.Features.Contracts.Commands.RenewContract;

public class RenewContractResponse
{
    public ContractDto NewContract { get; set; } = null!;
    public int RenewedFromContractID { get; set; }
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
}
