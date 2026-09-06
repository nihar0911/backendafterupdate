using Microsoft.EntityFrameworkCore;
using VendorManagementprojApplication.Contracts.Persistence;
using VendorManagementprojDomain.Entities;
using VendorManagementprojPersistence.Data;

namespace VendorManagementprojPersistence.Repositories;

public class VendorProductRepository : IVendorProductRepository
{
    private readonly VendorManagementDbContext _context;

    public VendorProductRepository(VendorManagementDbContext context)
    {
        _context = context;
    }

    public async Task<List<VendorProduct>> GetAllAsync()
    {
        return await _context.VendorProducts
            .ToListAsync();
    }

    public async Task<VendorProduct?> GetByIdAsync(
        int vendorProductID)
    {
        return await _context.VendorProducts
            .FirstOrDefaultAsync(vp =>
                vp.VendorProductID == vendorProductID);
    }

    public async Task<VendorProduct?> GetByVendorAndProductAsync(
        int vendorID,
        int productID)
    {
        return await _context.VendorProducts
            .FirstOrDefaultAsync(vp =>
                vp.VendorID == vendorID &&
                vp.ProductID == productID);
    }

    public async Task<List<VendorProduct>> GetByProductNameAsync(
        string productName)
    {
        return await _context.VendorProducts
            .Include(vp => vp.Product)
            .Where(vp =>
                vp.Product.ProductName.Contains(productName) &&
                vp.Status == "Active")
            .ToListAsync();
    }

        public async Task<List<VendorProduct>> GetEligibleVendorsForProductAsync(
        int productID,
        int outletID)
    {
        return await _context.VendorProducts
            .Include(vp => vp.Product)
            .Where(vp =>
                vp.ProductID == productID &&
                vp.Status == "Active" &&
                vp.Product != null &&
                vp.Product.Status == "Active" &&
                _context.Vendors.Any(v => v.VendorID == vp.VendorID && v.Status == "Active"))
            .ToListAsync();
    }
    public async Task<bool> IsVendorEligibleAsync(
    int vendorID,
    int productID,
    int outletID)
    {
        var today = DateTime.Now;

        return await _context.ContractVendorAllocations
            .AnyAsync(a =>
                a.VendorID == vendorID &&
                a.Status == "Active" &&
                a.Contract != null &&
                a.Contract.ProductID == productID &&
                a.Contract.OutletID == outletID &&
                a.Contract.Status == "Active" &&
                a.Contract.StartDate <= today &&
                a.Contract.EndDate >= today);
    }
    public async Task<VendorProduct> AddAsync(
        VendorProduct vendorProduct)
    {
        var exists =
            await _context.VendorProducts
                .AnyAsync(vp =>
                    vp.VendorID == vendorProduct.VendorID &&
                    vp.ProductID == vendorProduct.ProductID);

        if (exists)
        {
            throw new InvalidOperationException(
                $"VendorProduct already exists for VendorID={vendorProduct.VendorID}, ProductID={vendorProduct.ProductID}");
        }

        vendorProduct.VendorProductID = 0;

        _context.VendorProducts.Add(vendorProduct);

        await _context.SaveChangesAsync();

        return vendorProduct;
    }

    public async Task<VendorProduct?> UpdateAsync(
        int vendorProductID,
        VendorProduct vendorProduct)
    {
        var existingVendorProduct =
            await _context.VendorProducts
                .FirstOrDefaultAsync(vp =>
                    vp.VendorProductID == vendorProductID);

        if (existingVendorProduct == null)
            return null;

        existingVendorProduct.VendorID =
            vendorProduct.VendorID;

        existingVendorProduct.ProductID =
            vendorProduct.ProductID;

        existingVendorProduct.UnitPrice =
            vendorProduct.UnitPrice;

        existingVendorProduct.EstimatedDeliveryDays =
            vendorProduct.EstimatedDeliveryDays;

        existingVendorProduct.Status =
            vendorProduct.Status;

        await _context.SaveChangesAsync();

        return existingVendorProduct;
    }

    public async Task<bool> DeleteAsync(
        int vendorProductID)
    {
        var vendorProduct =
            await _context.VendorProducts
                .FirstOrDefaultAsync(vp =>
                    vp.VendorProductID == vendorProductID);

        if (vendorProduct == null)
            return false;

        _context.VendorProducts.Remove(vendorProduct);

        await _context.SaveChangesAsync();

        return true;
    }
    public async Task<List<VendorProduct>> GetByProductNameForOutletAsync(
       string productName,
       int outletID)
    {
        return await _context.VendorProducts
            .Include(vp => vp.Product)
            .Where(vp =>
                vp.Product.ProductName.Contains(productName) &&
                vp.Status == "Active" &&
                vp.Product != null &&
                vp.Product.Status == "Active" &&
                _context.Vendors.Any(v => v.VendorID == vp.VendorID && v.Status == "Active"))
            .ToListAsync();
    }
}