using Microsoft.EntityFrameworkCore;
using VendorManagementprojApplication.Contracts.Persistence;
using VendorManagementprojDomain.Entities;
using VendorManagementprojPersistence.Data;

namespace VendorManagementprojPersistence.Repositories;

public class DiscountRepository : IDiscountRepository
{
    private readonly VendorManagementDbContext _context;

    public DiscountRepository(VendorManagementDbContext context)
    {
        _context = context;
    }

    public async Task<List<Discount>> GetAllAsync()
    {
        return await _context.Discounts.ToListAsync();
    }

    public async Task<Discount?> GetByIdAsync(int discountID)
    {
        return await _context.Discounts
            .FirstOrDefaultAsync(d => d.DiscountID == discountID);
    }

    public async Task<Discount> AddAsync(Discount discount)
    {
        _context.Discounts.Add(discount);
        await _context.SaveChangesAsync();

        return discount;
    }

    public async Task<Discount?> UpdateAsync(
        int discountID,
        Discount discount)
    {
        var existingDiscount = await _context.Discounts
            .FirstOrDefaultAsync(d => d.DiscountID == discountID);

        if (existingDiscount == null)
            return null;

        existingDiscount.VendorID = discount.VendorID;
        existingDiscount.ProductID = discount.ProductID;
        existingDiscount.DiscountName = discount.DiscountName;
        existingDiscount.DiscountType = discount.DiscountType;
        existingDiscount.DiscountValue = discount.DiscountValue;
        existingDiscount.MinimumQuantity = discount.MinimumQuantity;
        existingDiscount.StartDate = discount.StartDate;
        existingDiscount.EndDate = discount.EndDate;
        existingDiscount.Status = discount.Status;

        await _context.SaveChangesAsync();

        return existingDiscount;
    }

    public async Task<bool> DeleteAsync(int discountID)
    {
        var discount = await _context.Discounts
            .FirstOrDefaultAsync(d => d.DiscountID == discountID);

        if (discount == null)
            return false;

        _context.Discounts.Remove(discount);

        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<bool> VendorProductExistsAsync(
        int vendorID,
        int productID)
    {
        return await _context.VendorProducts
            .AnyAsync(vp =>
                vp.VendorID == vendorID &&
                vp.ProductID == productID);
    }

    public async Task<VendorProduct?> GetVendorProductAsync(
        int vendorID,
        int productID)
    {
        return await _context.VendorProducts
            .FirstOrDefaultAsync(vp =>
                vp.VendorID == vendorID &&
                vp.ProductID == productID);
    }

    public async Task<Discount?> GetActiveDiscountAsync(
        int vendorID,
        int productID,
        DateTime date)
    {
        return await _context.Discounts
            .Where(d =>
                d.VendorID == vendorID &&
                d.ProductID == productID &&
                d.Status == "Active" &&
                d.StartDate <= date &&
                d.EndDate >= date)
            .OrderByDescending(d => d.DiscountValue)
            .FirstOrDefaultAsync();
    }
    public async Task<bool> HasOverlappingDiscountAsync(
    int vendorID,
    int productID,
    DateTime startDate,
    DateTime endDate,
    int? excludeDiscountID = null)
    {
        var query = _context.Discounts
            .Where(d =>
                d.VendorID == vendorID &&
                d.ProductID == productID &&
                d.StartDate <= endDate &&
                d.EndDate >= startDate);

        if (excludeDiscountID.HasValue)
        {
            query = query.Where(
                d => d.DiscountID != excludeDiscountID.Value);
        }

        return await query.AnyAsync();
    }
}