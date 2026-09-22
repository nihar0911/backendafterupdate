using System.Collections.Generic;
using System.Threading.Tasks;
using VendorManagementproj.Application.DTOs;

namespace VendorManagementproj.Application.Contracts.Services;

public interface IVendorPerformanceService
{
    Task<List<VendorPerformanceSummaryDto>> GetOrganizationVendorsPerformanceAsync(int? organizationId = null);
    Task<VendorPerformanceSummaryDto?> GetVendorPerformanceAsync(int vendorId, int? organizationId = null);
    Task<Dictionary<int, VendorPerformanceSummaryDto>> GetVendorsPerformanceBatchAsync(List<int> vendorIds, int? organizationId = null);
}
