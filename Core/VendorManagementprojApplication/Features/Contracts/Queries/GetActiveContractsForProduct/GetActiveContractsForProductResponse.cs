using System.Collections.Generic;
using VendorManagementprojApplication.DTOs;

namespace VendorManagementprojApplication.Features.Contracts.Queries.GetActiveContractsForProduct;

public class GetActiveContractsForProductResponse
{
    public List<ContractDto> Contracts { get; set; } = new();
}
