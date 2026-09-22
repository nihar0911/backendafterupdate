using System.Collections.Generic;
using VendorManagementproj.Application.DTOs;

namespace VendorManagementproj.Application.Features.Contracts.Queries.GetActiveContractsForProduct;

public class GetActiveContractsForProductResponse
{
    public List<ContractDto> Contracts { get; set; } = new();
}
