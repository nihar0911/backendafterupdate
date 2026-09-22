using System.Collections.Generic;
using VendorManagementprojApplication.DTOs;

namespace VendorManagementprojApplication.Features.PurchaseRequests.Queries.GetVendorProcurementOpportunities;

public class GetVendorProcurementOpportunitiesResponse
{
    public List<VendorProcurementOpportunityDto> Opportunities { get; set; } = new();
}
