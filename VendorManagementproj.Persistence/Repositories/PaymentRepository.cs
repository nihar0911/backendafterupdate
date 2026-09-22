using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using VendorManagementprojApplication.Contracts.Persistence;
using VendorManagementprojDomain.Entities;
using VendorManagementprojPersistence.Data;

namespace VendorManagementprojPersistence.Repositories;

public class PaymentRepository : IPaymentRepository
{
    private readonly VendorManagementDbContext _context;

    public PaymentRepository(VendorManagementDbContext context)
    {
        _context = context;
    }

    public async Task<Payment> AddAsync(Payment payment)
    {
        await _context.Payments.AddAsync(payment);
        await _context.SaveChangesAsync();
        return payment;
    }

    public async Task<Payment?> GetByIdAsync(int paymentID)
    {
        return await _context.Payments
            .Include(p => p.Invoice)
                .ThenInclude(i => i!.PurchaseOrder)
                    .ThenInclude(po => po!.Outlet)
                        .ThenInclude(o => o!.Organization)
            .Include(p => p.Invoice)
                .ThenInclude(i => i!.Vendor)
            .Include(p => p.Invoice)
                .ThenInclude(i => i!.Outlet)
            .FirstOrDefaultAsync(p => p.PaymentID == paymentID);
    }

    public async Task<Payment?> GetByInvoiceIdAsync(int invoiceID)
    {
        return await _context.Payments
            .Include(p => p.Invoice)
                .ThenInclude(i => i!.PurchaseOrder)
                    .ThenInclude(po => po!.Outlet)
                        .ThenInclude(o => o!.Organization)
            .Include(p => p.Invoice)
                .ThenInclude(i => i!.Vendor)
            .Include(p => p.Invoice)
                .ThenInclude(i => i!.Outlet)
            .FirstOrDefaultAsync(p => p.InvoiceID == invoiceID);
    }

    public async Task<List<Payment>> GetAllAsync()
    {
        return await _context.Payments
            .Include(p => p.Invoice)
                .ThenInclude(i => i!.PurchaseOrder)
                    .ThenInclude(po => po!.Outlet)
                        .ThenInclude(o => o!.Organization)
            .Include(p => p.Invoice)
                .ThenInclude(i => i!.Vendor)
            .Include(p => p.Invoice)
                .ThenInclude(i => i!.Outlet)
            .OrderByDescending(p => p.PaymentDate)
            .ToListAsync();
    }

    public async Task<List<Payment>> GetByOrganizationIdAsync(int organizationID)
    {
        return await _context.Payments
            .Include(p => p.Invoice)
                .ThenInclude(i => i!.PurchaseOrder)
                    .ThenInclude(po => po!.Outlet)
                        .ThenInclude(o => o!.Organization)
            .Include(p => p.Invoice)
                .ThenInclude(i => i!.Vendor)
            .Include(p => p.Invoice)
                .ThenInclude(i => i!.Outlet)
            .Where(p => p.Invoice != null &&
                        p.Invoice.PurchaseOrder != null &&
                        p.Invoice.PurchaseOrder.Outlet != null &&
                        p.Invoice.PurchaseOrder.Outlet.OrganizationID == organizationID)
            .OrderByDescending(p => p.PaymentDate)
            .ToListAsync();
    }

    public async Task<List<Payment>> GetByVendorIdAsync(int vendorID)
    {
        return await _context.Payments
            .Include(p => p.Invoice)
                .ThenInclude(i => i!.PurchaseOrder)
                    .ThenInclude(po => po!.Outlet)
                        .ThenInclude(o => o!.Organization)
            .Include(p => p.Invoice)
                .ThenInclude(i => i!.Vendor)
            .Include(p => p.Invoice)
                .ThenInclude(i => i!.Outlet)
            .Where(p => p.Invoice != null && p.Invoice.VendorID == vendorID)
            .OrderByDescending(p => p.PaymentDate)
            .ToListAsync();
    }
}