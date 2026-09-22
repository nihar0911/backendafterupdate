using Microsoft.EntityFrameworkCore;
using VendorManagementproj.Application.Contracts.Persistence;
using VendorManagementproj.Domain.Entities;
using VendorManagementproj.Persistence.Data;

namespace VendorManagementproj.Persistence.Repositories;

public class OrganizationRepository : IOrganizationRepository
{
    private readonly VendorManagementDbContext _context;

    public OrganizationRepository(VendorManagementDbContext context)
    {
        _context = context;
    }

    public async Task<List<Organization>> GetAllAsync()
    {
        return await _context.Organizations
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<Organization?> GetByIdAsync(int id)
    {
        return await _context.Organizations
            .FirstOrDefaultAsync(o => o.OrganizationID == id);
    }

    public async Task<Organization?> GetByEmailAsync(string email)
    {
        return await _context.Organizations
            .FirstOrDefaultAsync(o => o.Email == email);
    }

    public async Task AddAsync(Organization organization)
    {
        await _context.Organizations.AddAsync(organization);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(Organization organization)
    {
        _context.Organizations.Update(organization);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(Organization organization)
    {
        organization.Status = "Inactive";
        await _context.SaveChangesAsync();
    }
}