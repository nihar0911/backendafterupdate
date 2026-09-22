using System.Collections.Generic;
using VendorManagementproj.Application.DTOs;

namespace VendorManagementproj.Application.Features.Contracts.Queries.GetContractEligibleVendors;

public class GetContractEligibleVendorsResponse
{
    public int OutletID { get; set; }
    public int ProductID { get; set; }
    public bool HasActiveContracts { get; set; }
    public List<VendorRecommendationDto> Vendors { get; set; } = new();
}
