using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using VendorManagementprojApplication.Contracts.Persistence;
using VendorManagementprojDomain.Entities;
using VendorManagementprojPersistence.Data;

namespace VendorManagementprojPersistence.Repositories;

public class VendorOpportunityResponseRepository : IVendorOpportunityResponseRepository
{
    private readonly VendorManagementDbContext _context;

    public VendorOpportunityResponseRepository(VendorManagementDbContext context)
    {
        _context = context;
    }

    public async Task<VendorOpportunityResponse?> GetByRequestAndVendorAsync(int requestId, int vendorId, int productId)
    {
        return await _context.VendorOpportunityResponses
            .FirstOrDefaultAsync(r => r.RequestID == requestId && r.VendorID == vendorId && r.ProductID == productId);
    }

    public async Task<VendorOpportunityResponse?> GetByRequestItemAndVendorAsync(int requestItemId, int vendorId)
    {
        return await _context.VendorOpportunityResponses
            .FirstOrDefaultAsync(r => r.RequestItemID == requestItemId && r.VendorID == vendorId);
    }

    public async Task<List<VendorOpportunityResponse>> GetByVendorIdAsync(int vendorId)
    {
        return await _context.VendorOpportunityResponses
            .Where(r => r.VendorID == vendorId)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<List<VendorOpportunityResponse>> GetByRequestIdAsync(int requestId)
    {
        return await _context.VendorOpportunityResponses
            .Where(r => r.RequestID == requestId)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<List<VendorOpportunityResponse>> GetAllAsync()
    {
        return await _context.VendorOpportunityResponses
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<VendorOpportunityResponse> AddAsync(VendorOpportunityResponse response)
    {
        _context.VendorOpportunityResponses.Add(response);
        await _context.SaveChangesAsync();
        return response;
    }

    public async Task UpdateAsync(VendorOpportunityResponse response)
    {
        _context.VendorOpportunityResponses.Update(response);
        await _context.SaveChangesAsync();
    }
}
