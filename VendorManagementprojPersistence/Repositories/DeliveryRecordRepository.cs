using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using VendorManagementprojApplication.Contracts.Persistence;
using VendorManagementprojDomain.Entities;
using VendorManagementprojPersistence.Data;

namespace VendorManagementprojPersistence.Repositories;

public class DeliveryRecordRepository : IDeliveryRecordRepository
{
    private readonly VendorManagementDbContext _context;

    public DeliveryRecordRepository(VendorManagementDbContext context)
    {
        _context = context;
    }

    public async Task<DeliveryRecord> AddAsync(
        DeliveryRecord deliveryRecord)
    {
        _context.DeliveryRecords.Add(deliveryRecord);

        await _context.SaveChangesAsync();

        return deliveryRecord;
    }

    public async Task<DeliveryRecord?> GetByIdAsync(
        int deliveryRecordID)
    {
        return await _context.DeliveryRecords
            .Include(d => d.PurchaseOrder)
            .Include(d => d.PurchaseOrderItem)
                .ThenInclude(i => i!.Product)
            .Include(d => d.ConfirmedByUser)
            .FirstOrDefaultAsync(
                d => d.DeliveryRecordID == deliveryRecordID);
    }

    public async Task<List<DeliveryRecord>> GetAllAsync()
    {
        return await _context.DeliveryRecords
            .Include(d => d.PurchaseOrder)
            .Include(d => d.PurchaseOrderItem)
                .ThenInclude(i => i!.Product)
            .Include(d => d.ConfirmedByUser)
            .ToListAsync();
    }

    public async Task<List<DeliveryRecord>> GetByPurchaseOrderIdAsync(
        int purchaseOrderID)
    {
        return await _context.DeliveryRecords
            .Include(d => d.PurchaseOrderItem)
                .ThenInclude(i => i!.Product)
            .Include(d => d.ConfirmedByUser)
            .Where(d => d.PurchaseOrderID == purchaseOrderID)
            .ToListAsync();
    }

    public async Task<List<DeliveryRecord>> GetByVendorProductOutletAsync(int vendorID, int productID, int outletID)
    {
        return await _context.DeliveryRecords
            .Include(d => d.PurchaseOrder)
            .Include(d => d.PurchaseOrderItem)
                .ThenInclude(i => i!.Product)
            .Include(d => d.ConfirmedByUser)
            .Where(d =>
                d.PurchaseOrder != null &&
                d.PurchaseOrder.VendorID == vendorID &&
                d.PurchaseOrder.OutletID == outletID &&
                d.PurchaseOrderItem != null &&
                d.PurchaseOrderItem.ProductID == productID)
            .ToListAsync();
    }

    public async Task<DeliveryRecord?> UpdateAsync(
        DeliveryRecord deliveryRecord)
    {
        var existing =
            await _context.DeliveryRecords
                .FirstOrDefaultAsync(
                    d => d.DeliveryRecordID ==
                         deliveryRecord.DeliveryRecordID);

        if (existing == null)
            return null;

        existing.DeliveryDate =
            deliveryRecord.DeliveryDate;

        existing.OrderedQuantity =
            deliveryRecord.OrderedQuantity;

        existing.ReceivedQuantity =
            deliveryRecord.ReceivedQuantity;

        existing.SpoiledQuantity =
            deliveryRecord.SpoiledQuantity;

        existing.SpoilagePercentage =
            deliveryRecord.SpoilagePercentage;

        existing.Status =
            deliveryRecord.Status;

        existing.ConfirmedByUserID =
            deliveryRecord.ConfirmedByUserID;

        existing.ConfirmedAt =
            deliveryRecord.ConfirmedAt;

        await _context.SaveChangesAsync();

        return existing;
    }

    public async Task<List<DeliveryRecord>> GetConfirmedByVendorAsync(int vendorID)
    {
        return await _context.DeliveryRecords
            .Include(d => d.PurchaseOrder)
            .Include(d => d.PurchaseOrderItem)
                .ThenInclude(i => i!.Product)
            .Include(d => d.ConfirmedByUser)
            .Where(d =>
                d.PurchaseOrder != null &&
                d.PurchaseOrder.VendorID == vendorID &&
                d.Status == "Confirmed")
            .ToListAsync();
    }

    public async Task<List<DeliveryRecord>> GetConfirmedByVendorAndProductAsync(int vendorID, int productID)
    {
        return await _context.DeliveryRecords
            .Include(d => d.PurchaseOrder)
            .Include(d => d.PurchaseOrderItem)
                .ThenInclude(i => i!.Product)
            .Include(d => d.ConfirmedByUser)
            .Where(d =>
                d.PurchaseOrder != null &&
                d.PurchaseOrder.VendorID == vendorID &&
                d.PurchaseOrderItem != null &&
                d.PurchaseOrderItem.ProductID == productID &&
                d.Status == "Confirmed")
            .ToListAsync();
    }
}

