using VendorManagementprojApplication.DTOs;

namespace VendorManagementprojApplication.Features.Contracts.Queries.GetContractsByOutlet;

public class GetContractsByOutletResponse
{
    public List<ContractDto> Contracts { get; set; } = new();
}
