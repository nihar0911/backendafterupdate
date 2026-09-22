using VendorManagementproj.Application.DTOs;

namespace VendorManagementproj.Application.Features.Contracts.Queries.GetContractsByOutlet;

public class GetContractsByOutletResponse
{
    public List<ContractDto> Contracts { get; set; } = new();
}
