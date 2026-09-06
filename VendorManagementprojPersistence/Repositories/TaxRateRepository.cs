using Microsoft.EntityFrameworkCore;
using VendorManagementprojApplication.Contracts.Persistence;
using VendorManagementprojDomain.Entities;
using VendorManagementprojPersistence.Data;

namespace VendorManagementprojPersistence.Repositories;

public class TaxRateRepository : ITaxRateRepository
{
    private readonly VendorManagementDbContext _context;

    public TaxRateRepository(
        VendorManagementDbContext context)
    {
        _context = context;
    }

    public async Task<List<TaxRate>> GetAllAsync()
    {
        return await _context.TaxRates
            .ToListAsync();
    }

    public async Task<TaxRate?> GetByIdAsync(
        int taxRateID)
    {
        return await _context.TaxRates
            .FirstOrDefaultAsync(
                t => t.TaxRateID == taxRateID);
    }

    public async Task<TaxRate?> GetActiveTaxRateAsync()
    {
        return await _context.TaxRates
            .FirstOrDefaultAsync(
                t => t.Status == "Active");
    }

    public async Task<TaxRate> AddAsync(
        TaxRate taxRate)
    {
        _context.TaxRates.Add(taxRate);

        await _context.SaveChangesAsync();

        return taxRate;
    }

    public async Task<TaxRate?> UpdateAsync(
        int taxRateID,
        TaxRate taxRate)
    {
        var existingTaxRate =
            await _context.TaxRates
                .FirstOrDefaultAsync(
                    t => t.TaxRateID == taxRateID);

        if (existingTaxRate == null)
            return null;

        existingTaxRate.TaxName =
            taxRate.TaxName;

        existingTaxRate.Percentage =
            taxRate.Percentage;

        existingTaxRate.Status =
            taxRate.Status;

        await _context.SaveChangesAsync();

        return existingTaxRate;
    }

    public async Task<bool> DeleteAsync(
        int taxRateID)
    {
        var taxRate =
            await _context.TaxRates
                .FirstOrDefaultAsync(
                    t => t.TaxRateID == taxRateID);

        if (taxRate == null)
            return false;

        _context.TaxRates.Remove(taxRate);

        await _context.SaveChangesAsync();

        return true;
    }
}