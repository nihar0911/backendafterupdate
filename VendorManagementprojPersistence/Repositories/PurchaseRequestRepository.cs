using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using VendorManagementprojApplication.Contracts.Persistence;
using VendorManagementprojDomain.Entities;
using VendorManagementprojPersistence.Data;

namespace VendorManagementprojPersistence.Repositories;

public class PurchaseRequestRepository : IPurchaseRequestRepository
{
    private readonly VendorManagementDbContext _context;

    public PurchaseRequestRepository(VendorManagementDbContext context)
    {
        _context = context;
    }

    public async Task<List<PurchaseRequest>> GetAllAsync()
    {
        return await _context.PurchaseRequests
            .Include(r => r.Items)
                .ThenInclude(i => i.Product)
            .ToListAsync();
    }

    public async Task<PurchaseRequest?> GetByIdAsync(int requestID)
    {
        return await _context.PurchaseRequests
            .Include(r => r.Items)
                .ThenInclude(i => i.Product)
            .FirstOrDefaultAsync(r => r.RequestID == requestID);
    }

    public async Task<PurchaseRequest> AddAsync(
        PurchaseRequest purchaseRequest)
    {
        _context.PurchaseRequests.Add(purchaseRequest);

        await _context.SaveChangesAsync();

        return purchaseRequest;
    }

    public async Task<PurchaseRequest?> UpdateAsync(
        int requestID,
        PurchaseRequest purchaseRequest)
    {
        var existingRequest =
            await _context.PurchaseRequests
                .FirstOrDefaultAsync(
                    r => r.RequestID == requestID);

        if (existingRequest == null)
            return null;

        existingRequest.OutletID =
            purchaseRequest.OutletID;

        await _context.SaveChangesAsync();

        return existingRequest;
    }

    public async Task<bool> DeleteAsync(int requestID)
    {
        var purchaseRequest =
            await _context.PurchaseRequests
                .FirstOrDefaultAsync(
                    r => r.RequestID == requestID);

        if (purchaseRequest == null)
            return false;

        _context.PurchaseRequests.Remove(
            purchaseRequest);

        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<List<PurchaseRequestItem>> GetItemsByRequestIdAsync(
        int requestID)
    {
        return await _context.PurchaseRequestItems
            .Include(i => i.Product)
            .Where(i => i.RequestID == requestID)
            .ToListAsync();
    }

    public async Task<PurchaseRequestItem> AddItemAsync(
        PurchaseRequestItem item)
    {
        _context.PurchaseRequestItems.Add(item);

        await _context.SaveChangesAsync();

        await _context.Entry(item)
            .Reference(i => i.Product)
            .LoadAsync();

        return item;
    }

    public async Task<bool> DeleteItemAsync(int itemId)
    {
        var item =
            await _context.PurchaseRequestItems
                .Include(i => i.Request)
                .FirstOrDefaultAsync(
                    i => i.RequestItemID == itemId);

        if (item == null)
            return false;

        if (item.Request == null)
            return false;

        if (!string.Equals(
            item.Request.Status,
            "Pending",
            StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException(
                "Items can only be deleted from a pending purchase request.");
        }

        _context.PurchaseRequestItems.Remove(item);

        await _context.SaveChangesAsync();

        return true;
    }
}
