using Microsoft.EntityFrameworkCore;
using VendorManagementprojApplication.Contracts.Persistence;
using VendorManagementprojDomain.Entities;
using VendorManagementprojPersistence.Data;

namespace VendorManagementprojPersistence.Repositories;

public class VendorRepository : IVendorRepository
{
    private readonly VendorManagementDbContext _context;

    public VendorRepository(
        VendorManagementDbContext context)
    {
        _context = context;
    }

    public async Task<List<Vendor>> GetAllAsync()
    {
        return await _context.Vendors
            .ToListAsync();
    }

    public async Task<Vendor?> GetByIdAsync(
        int vendorID)
    {
        return await _context.Vendors
            .FirstOrDefaultAsync(
                v => v.VendorID == vendorID);
    }

    public async Task<Vendor> AddAsync(
        Vendor vendor)
    {
        _context.Vendors.Add(vendor);

        await _context.SaveChangesAsync();

        return vendor;
    }

    public async Task<Vendor?> UpdateAsync(
        int vendorID,
        Vendor vendor)
    {
        var existingVendor =
            await _context.Vendors
                .FirstOrDefaultAsync(
                    v => v.VendorID == vendorID);

        if (existingVendor == null)
            return null;

        existingVendor.VendorName =
            vendor.VendorName;

        existingVendor.Email =
            vendor.Email;

        existingVendor.Phone =
            vendor.Phone;

        existingVendor.Address =
            vendor.Address;

        existingVendor.Latitude =
            vendor.Latitude;

        existingVendor.Longitude =
            vendor.Longitude;

        existingVendor.GSTIN =
            vendor.GSTIN;

        existingVendor.Status =
            vendor.Status;

        await _context.SaveChangesAsync();

        return existingVendor;
    }

    public async Task<bool> DeleteAsync(
        int vendorID)
    {
        var vendor =
            await _context.Vendors
                .FirstOrDefaultAsync(
                    v => v.VendorID == vendorID);

        if (vendor == null)
            return false;

        _context.Vendors.Remove(vendor);

        await _context.SaveChangesAsync();

        return true;
    }
}