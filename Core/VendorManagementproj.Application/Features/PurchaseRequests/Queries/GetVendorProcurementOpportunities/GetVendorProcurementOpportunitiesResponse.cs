using System.Collections.Generic;
using VendorManagementproj.Application.DTOs;

namespace VendorManagementproj.Application.Features.PurchaseRequests.Queries.GetVendorProcurementOpportunities;

public class GetVendorProcurementOpportunitiesResponse
{
    public List<VendorProcurementOpportunityDto> Opportunities { get; set; } = new();
}
