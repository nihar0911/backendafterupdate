using Microsoft.EntityFrameworkCore;
using VendorManagementprojApplication.Contracts.Persistence;
using VendorManagementprojDomain.Entities;
using VendorManagementprojPersistence.Data;

namespace VendorManagementprojPersistence.Repositories;

public class VendorFeedbackRepository : IVendorFeedbackRepository
{
    private readonly VendorManagementDbContext _context;

    public VendorFeedbackRepository(
        VendorManagementDbContext context)
    {
        _context = context;
    }

    public async Task<VendorFeedback> AddAsync(
        VendorFeedback feedback)
    {
        await _context.VendorFeedbacks.AddAsync(feedback);

        await _context.SaveChangesAsync();

        return feedback;
    }

    public async Task<VendorFeedback?> GetByIdAsync(
        int feedbackID)
    {
        return await _context.VendorFeedbacks
            .Include(f => f.Vendor)
            .Include(f => f.Outlet)
            .Include(f => f.PurchaseOrder)
            .Include(f => f.POItem).ThenInclude(pi => pi!.Product)
            .Include(f => f.RatedByUser)
            .FirstOrDefaultAsync(
                f => f.FeedbackID == feedbackID);
    }

    public async Task<List<VendorFeedback>> GetAllAsync()
    {
        return await _context.VendorFeedbacks
            .Include(f => f.Vendor)
            .Include(f => f.Outlet)
            .Include(f => f.PurchaseOrder)
            .Include(f => f.POItem).ThenInclude(pi => pi!.Product)
            .Include(f => f.RatedByUser)
            .ToListAsync();
    }

    public async Task<List<VendorFeedback>> GetByVendorIdAsync(
        int vendorID,
        int? organizationID = null)
    {
        var query = _context.VendorFeedbacks
            .Include(f => f.Vendor)
            .Include(f => f.Outlet)
            .Include(f => f.PurchaseOrder)
            .Include(f => f.POItem).ThenInclude(pi => pi!.Product)
            .Include(f => f.RatedByUser)
            .Where(f => f.VendorID == vendorID);

        if (organizationID.HasValue && organizationID.Value > 0)
        {
            query = query.Where(f => f.Outlet != null && f.Outlet.OrganizationID == organizationID.Value);
        }

        return await query.OrderByDescending(f => f.FeedbackDate).ToListAsync();
    }

    public async Task<List<VendorFeedback>> GetByOrganizationIdAsync(
        int organizationID)
    {
        return await _context.VendorFeedbacks
            .Include(f => f.Vendor)
            .Include(f => f.Outlet)
            .Include(f => f.PurchaseOrder)
            .Include(f => f.POItem).ThenInclude(pi => pi!.Product)
            .Include(f => f.RatedByUser)
            .Where(f => f.Outlet != null && f.Outlet.OrganizationID == organizationID)
            .OrderByDescending(f => f.FeedbackDate)
            .ToListAsync();
    }

    public async Task<bool> ExistsForPOItemAsync(
        int purchaseOrderID,
        int poItemID)
    {
        return await _context.VendorFeedbacks
            .AnyAsync(f =>
                f.PurchaseOrderID == purchaseOrderID &&
                f.POItemID == poItemID);
    }
}


