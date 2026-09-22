using System.Collections.Generic;
using VendorManagementproj.Application.DTOs;

namespace VendorManagementproj.Application.Features.Contracts.Queries.GetContractsByOrganizationId;

public class GetContractsByOrganizationIdResponse
{
    public List<ContractDto> Contracts { get; set; } = new();
}
