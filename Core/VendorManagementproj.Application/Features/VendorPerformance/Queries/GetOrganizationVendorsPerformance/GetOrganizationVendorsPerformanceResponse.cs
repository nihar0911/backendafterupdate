using System.Collections.Generic;
using VendorManagementproj.Application.DTOs;

namespace VendorManagementproj.Application.Features.VendorPerformance.Queries.GetOrganizationVendorsPerformance;

public class GetOrganizationVendorsPerformanceResponse
{
    public List<VendorPerformanceSummaryDto> VendorPerformances { get; set; } = new();
}
