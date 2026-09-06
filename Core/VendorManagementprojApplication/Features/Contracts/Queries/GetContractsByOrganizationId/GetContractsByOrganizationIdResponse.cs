using System.Collections.Generic;
using VendorManagementprojApplication.DTOs;

namespace VendorManagementprojApplication.Features.Contracts.Queries.GetContractsByOrganizationId;

public class GetContractsByOrganizationIdResponse
{
    public List<ContractDto> Contracts { get; set; } = new();
}
