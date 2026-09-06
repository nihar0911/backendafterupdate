using Microsoft.EntityFrameworkCore;
using VendorManagementprojApplication.Contracts.Persistence;
using VendorManagementprojDomain.Entities;
using VendorManagementprojPersistence.Data;

namespace VendorManagementprojPersistence.Repositories;

public class OutletRepository : IOutletRepository
{
    private readonly VendorManagementDbContext _context;

    public OutletRepository(VendorManagementDbContext context)
    {
        _context = context;
    }

    public async Task<List<Outlet>> GetAllAsync()
    {
        return await _context.Outlets
            .Include(o => o.Organization)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<Outlet?> GetByIdAsync(int id)
    {
        return await _context.Outlets
            .Include(o => o.Organization)
            .FirstOrDefaultAsync(o => o.OutletID == id);
    }

    public async Task<List<Outlet>> GetByOrganizationIdAsync(int organizationId)
    {
        return await _context.Outlets
            .Include(o => o.Organization)
            .Where(o => o.OrganizationID == organizationId)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<Outlet> AddAsync(Outlet outlet)
    {
        _context.Outlets.Add(outlet);
        await _context.SaveChangesAsync();

        return outlet;
    }

    public async Task<Outlet> UpdateAsync(Outlet outlet)
    {
        _context.Outlets.Update(outlet);
        await _context.SaveChangesAsync();

        return outlet;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var outlet = await _context.Outlets
            .FirstOrDefaultAsync(o => o.OutletID == id);

        if (outlet == null)
            return false;

        _context.Outlets.Remove(outlet);
        await _context.SaveChangesAsync();

        return true;
    }
}