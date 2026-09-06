using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using VendorManagementprojApplication.Contracts.Persistence;
using VendorManagementprojDomain.Entities;
using VendorManagementprojPersistence.Data;

namespace VendorManagementprojPersistence.Repositories;

public class InvoiceRepository : IInvoiceRepository
{
    private readonly VendorManagementDbContext _context;

    public InvoiceRepository(VendorManagementDbContext context)
    {
        _context = context;
    }

    public async Task<Invoice> AddAsync(Invoice invoice)
    {
        await _context.Invoices.AddAsync(invoice);
        await _context.SaveChangesAsync();
        return invoice;
    }

    public async Task<Invoice?> GetByIdAsync(int invoiceID)
    {
        return await _context.Invoices
            .Include(i => i.Items)
                .ThenInclude(it => it.Product)
            .Include(i => i.PurchaseOrder)
                .ThenInclude(po => po!.Request)
            .Include(i => i.PurchaseOrder)
                .ThenInclude(po => po!.Items)
            .Include(i => i.Vendor)
            .Include(i => i.Outlet)
                .ThenInclude(o => o!.Organization)
            .FirstOrDefaultAsync(i => i.InvoiceID == invoiceID);
    }

    public async Task<List<Invoice>> GetAllAsync()
    {
        return await _context.Invoices
            .Include(i => i.Items)
                .ThenInclude(it => it.Product)
            .Include(i => i.PurchaseOrder)
                .ThenInclude(po => po!.Request)
            .Include(i => i.PurchaseOrder)
                .ThenInclude(po => po!.Items)
            .Include(i => i.Vendor)
            .Include(i => i.Outlet)
                .ThenInclude(o => o!.Organization)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<Invoice?> GetByPurchaseOrderIdAsync(int purchaseOrderID)
    {
        return await _context.Invoices
            .Include(i => i.Items)
                .ThenInclude(it => it.Product)
            .Include(i => i.PurchaseOrder)
            .Include(i => i.Vendor)
            .Include(i => i.Outlet)
            .FirstOrDefaultAsync(i => i.PurchaseOrderID == purchaseOrderID);
    }

    public async Task<Invoice?> UpdateAsync(Invoice invoice)
    {
        _context.Invoices.Update(invoice);
        await _context.SaveChangesAsync();
        return invoice;
    }
}