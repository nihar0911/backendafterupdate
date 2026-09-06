using System.Collections.Generic;
using System.Threading.Tasks;
using VendorManagementprojDomain.Entities;

namespace VendorManagementprojApplication.Contracts.Persistence;

public interface IVendorOpportunityResponseRepository
{
    Task<VendorOpportunityResponse?> GetByRequestAndVendorAsync(int requestId, int vendorId, int productId);
    Task<List<VendorOpportunityResponse>> GetByVendorIdAsync(int vendorId);
    Task<List<VendorOpportunityResponse>> GetByRequestIdAsync(int requestId);
    Task<List<VendorOpportunityResponse>> GetAllAsync();
    Task<VendorOpportunityResponse> AddAsync(VendorOpportunityResponse response);
    Task UpdateAsync(VendorOpportunityResponse response);
}
