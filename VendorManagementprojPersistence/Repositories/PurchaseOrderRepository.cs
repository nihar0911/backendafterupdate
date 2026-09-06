using Microsoft.EntityFrameworkCore;
using VendorManagementprojApplication.Contracts.Persistence;
using VendorManagementprojDomain.Entities;
using VendorManagementprojPersistence.Data;

namespace VendorManagementprojPersistence.Repositories;

public class PurchaseOrderRepository : IPurchaseOrderRepository
{
    private readonly VendorManagementDbContext _context;

    public PurchaseOrderRepository(VendorManagementDbContext context)
    {
        _context = context;
    }

    public async Task<PurchaseOrder> AddAsync(
        PurchaseOrder purchaseOrder)
    {
        _context.PurchaseOrders.Add(purchaseOrder);

        await _context.SaveChangesAsync();

        return purchaseOrder;
    }

    public async Task<PurchaseOrder?> GetByIdAsync(
        int purchaseOrderID)
    {
        return await _context.PurchaseOrders
            .Include(p => p.Items)
            .Include(p => p.Outlet)
            .Include(p => p.Vendor)
            .FirstOrDefaultAsync(
                p => p.PurchaseOrderID == purchaseOrderID);
    }

    public async Task<List<PurchaseOrder>> GetAllAsync()
    {
        return await _context.PurchaseOrders
            .Include(p => p.Items)
            .Include(p => p.Outlet)
            .Include(p => p.Vendor)
            .ToListAsync();
    }

    public async Task<List<PurchaseOrder>> GetPendingByVendorAsync(
        int vendorID)
    {
        return await _context.PurchaseOrders
            .Include(p => p.Items)
            .Include(p => p.Outlet)
            .Include(p => p.Vendor)
            .Where(p =>
                p.VendorID == vendorID &&
                p.Status == "Pending")
            .ToListAsync();
    }

    public async Task<PurchaseOrder?> GetByQuotationIdAsync(
        int quotationID)
    {
        return await _context.PurchaseOrders
            .Include(po => po.Items)
            .Include(po => po.Outlet)
            .Include(po => po.Vendor)
            .FirstOrDefaultAsync(
                po => po.QuotationID == quotationID);
    }

    public async Task<PurchaseOrder?> UpdateAsync(
        PurchaseOrder purchaseOrder)
    {
        var existingPurchaseOrder =
            await _context.PurchaseOrders
                .FirstOrDefaultAsync(
                    p => p.PurchaseOrderID ==
                         purchaseOrder.PurchaseOrderID);

        if (existingPurchaseOrder == null)
            return null;

        existingPurchaseOrder.ExpectedDeliveryDate =
            purchaseOrder.ExpectedDeliveryDate;

        existingPurchaseOrder.ActualDeliveryDate =
            purchaseOrder.ActualDeliveryDate;

        existingPurchaseOrder.DeliveryStatus =
            purchaseOrder.DeliveryStatus;

        existingPurchaseOrder.Status =
            purchaseOrder.Status;

        await _context.SaveChangesAsync();

        return existingPurchaseOrder;
    }

    public async Task<bool> DeleteAsync(
        int purchaseOrderID)
    {
        var purchaseOrder =
            await _context.PurchaseOrders
                .FirstOrDefaultAsync(
                    p => p.PurchaseOrderID ==
                         purchaseOrderID);

        if (purchaseOrder == null)
            return false;

        _context.PurchaseOrders.Remove(
            purchaseOrder);

        await _context.SaveChangesAsync();

        return true;
    }
}
