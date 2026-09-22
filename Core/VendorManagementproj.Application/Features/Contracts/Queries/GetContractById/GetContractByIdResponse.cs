using VendorManagementproj.Application.DTOs;

namespace VendorManagementproj.Application.Features.Contracts.Queries.GetContractById;

public class GetContractByIdResponse
{
    public ContractDto? Contract { get; set; }
}
