using Microsoft.EntityFrameworkCore;
using VendorManagementprojApplication.Contracts.Persistence;
using VendorManagementprojDomain.Entities;
using VendorManagementprojPersistence.Data;

namespace VendorManagementprojPersistence.Repositories;

public class ComplaintRepository : IComplaintRepository
{
    private readonly VendorManagementDbContext _context;

    public ComplaintRepository(VendorManagementDbContext context)
    {
        _context = context;
    }

    public async Task<Complaint> AddAsync(Complaint complaint)
    {
        await _context.Complaints.AddAsync(complaint);

        await _context.SaveChangesAsync();

        return complaint;
    }

    public async Task<Complaint?> GetByIdAsync(
        int complaintID)
    {
        return await _context.Complaints
            .Include(c => c.Vendor)
            .Include(c => c.Outlet)
            .Include(c => c.PurchaseOrder)
            .Include(c => c.POItem)
            .Include(c => c.Product)
            .Include(c => c.RaisedByUser)
            .FirstOrDefaultAsync(
                c => c.ComplaintID == complaintID);
    }

    public async Task<List<Complaint>> GetAllAsync()
    {
        return await _context.Complaints
            .Include(c => c.Vendor)
            .Include(c => c.Outlet)
            .Include(c => c.PurchaseOrder)
            .Include(c => c.POItem)
            .Include(c => c.Product)
            .Include(c => c.RaisedByUser)
            .ToListAsync();
    }

    public async Task<Complaint?> UpdateAsync(
        Complaint complaint)
    {
        var existingComplaint =
            await _context.Complaints
                .FirstOrDefaultAsync(
                    c => c.ComplaintID == complaint.ComplaintID);

        if (existingComplaint == null)
            return null;

        existingComplaint.ComplaintType =
            complaint.ComplaintType;

        existingComplaint.Description =
            complaint.Description;

        existingComplaint.Severity =
            complaint.Severity;

        existingComplaint.Status =
            complaint.Status;

        await _context.SaveChangesAsync();

        return existingComplaint;
    }
}