using VendorManagementprojApplication.DTOs;

namespace VendorManagementprojApplication.Features.Contracts.Queries.GetAllContracts;

public class GetAllContractsResponse
{
    public List<ContractDto> Contracts { get; set; } = new();
}
