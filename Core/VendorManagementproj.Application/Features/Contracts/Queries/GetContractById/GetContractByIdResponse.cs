using VendorManagementprojApplication.DTOs;

namespace VendorManagementprojApplication.Features.Contracts.Queries.GetContractById;

public class GetContractByIdResponse
{
    public ContractDto? Contract { get; set; }
}
