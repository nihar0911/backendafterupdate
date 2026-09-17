using System.Collections.Generic;
using VendorManagementprojApplication.DTOs;

namespace VendorManagementprojApplication.Features.VendorPerformance.Queries.GetOrganizationVendorsPerformance;

public class GetOrganizationVendorsPerformanceResponse
{
    public List<VendorPerformanceSummaryDto> VendorPerformances { get; set; } = new();
}
