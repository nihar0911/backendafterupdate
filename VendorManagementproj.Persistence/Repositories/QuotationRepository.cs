using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using VendorManagementprojApplication.Contracts.Persistence;
using VendorManagementprojDomain.Entities;
using VendorManagementprojPersistence.Data;

namespace VendorManagementprojPersistence.Repositories;

public class QuotationRepository : IQuotationRepository
{
    private readonly VendorManagementDbContext _context;

    public QuotationRepository(VendorManagementDbContext context)
    {
        _context = context;
    }

    public async Task<List<Quotation>> GetAllAsync()
    {
        return await _context.Quotations
            .Include(q => q.QuotationItems)
            .Include(q => q.Request)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<Quotation?> GetByIdAsync(int quotationID)
    {
        return await _context.Quotations
            .Include(q => q.QuotationItems)
            .Include(q => q.Request)
            .FirstOrDefaultAsync(q => q.QuotationID == quotationID);
    }

    public async Task<List<Quotation>> GetByRequestAndVendorAsync(int requestId, int vendorId)
    {
        return await _context.Quotations
            .Include(q => q.QuotationItems)
            .Where(q => q.RequestID == requestId && q.VendorID == vendorId)
            .ToListAsync();
    }

    public async Task<Quotation> AddAsync(Quotation quotation)
    {
        _context.Quotations.Add(quotation);
        await _context.SaveChangesAsync();
        return quotation;
    }

    public async Task<Quotation?> UpdateAsync(
        int quotationID,
        Quotation quotation)
    {
        var existingQuotation = await _context.Quotations
            .Include(q => q.QuotationItems)
            .Include(q => q.Request)
            .FirstOrDefaultAsync(q => q.QuotationID == quotationID);

        if (existingQuotation == null)
            return null;

        existingQuotation.RequestID = quotation.RequestID;
        existingQuotation.VendorID = quotation.VendorID;
        existingQuotation.ValidUntil = quotation.ValidUntil;
        existingQuotation.Status = quotation.Status;

        await _context.SaveChangesAsync();

        return existingQuotation;
    }

    public async Task<bool> DeleteAsync(int quotationID)
    {
        var quotation = await _context.Quotations
            .FirstOrDefaultAsync(q => q.QuotationID == quotationID);

        if (quotation == null)
            return false;

        _context.Quotations.Remove(quotation);
        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<List<Quotation>> GetByOutletIdAsync(int outletId)
    {
        return await _context.Quotations
            .Where(q => q.Request != null && q.Request.OutletID == outletId)
            .Include(q => q.QuotationItems)
            .Include(q => q.Request)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<List<Quotation>> GetByOutletIdsAsync(IEnumerable<int> outletIds)
    {
        return await _context.Quotations
            .Where(q => q.Request != null && outletIds.Contains(q.Request.OutletID))
            .Include(q => q.QuotationItems)
            .Include(q => q.Request)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<List<Quotation>> GetByVendorIdAsync(int vendorId)
    {
        return await _context.Quotations
            .Where(q => q.VendorID == vendorId)
            .Include(q => q.QuotationItems)
            .Include(q => q.Request)
            .AsNoTracking()
            .ToListAsync();
    }
}
