using System.Collections.Generic;
using System.Threading.Tasks;
using VendorManagementprojApplication.DTOs;

namespace VendorManagementprojApplication.Contracts.Services;

public interface IVendorPerformanceService
{
    Task<List<VendorPerformanceSummaryDto>> GetOrganizationVendorsPerformanceAsync(int? organizationId = null);
    Task<VendorPerformanceSummaryDto?> GetVendorPerformanceAsync(int vendorId, int? organizationId = null);
}
