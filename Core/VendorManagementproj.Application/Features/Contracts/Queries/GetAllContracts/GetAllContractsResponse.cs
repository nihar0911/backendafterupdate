using VendorManagementproj.Application.DTOs;

namespace VendorManagementproj.Application.Features.Contracts.Queries.GetAllContracts;

public class GetAllContractsResponse
{
    public List<ContractDto> Contracts { get; set; } = new();
}
