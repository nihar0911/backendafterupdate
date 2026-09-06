using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using VendorManagementprojApplication.Contracts.Persistence;
using VendorManagementprojDomain.Entities;
using VendorManagementprojPersistence.Data;

namespace VendorManagementprojPersistence.Repositories;

public class ContractRepository : IContractRepository
{
    private readonly VendorManagementDbContext _context;

    public ContractRepository(VendorManagementDbContext context)
    {
        _context = context;
    }

    public async Task<Contract> AddAsync(Contract contract)
    {
        _context.Contracts.Add(contract);

        await _context.SaveChangesAsync();

        return contract;
    }

    public async Task<Contract?> GetByIdAsync(int contractID)
    {
        return await _context.Contracts
            .Include(c => c.Outlet)
                .ThenInclude(o => o!.Organization)
            .Include(c => c.Product)
            .Include(c => c.VendorAllocations)
                .ThenInclude(a => a!.Vendor)
            .FirstOrDefaultAsync(c => c.ContractID == contractID);
    }

    public async Task<List<Contract>> GetAllAsync()
    {
        return await _context.Contracts
            .Include(c => c.Outlet)
                .ThenInclude(o => o!.Organization)
            .Include(c => c.Product)
            .Include(c => c.VendorAllocations)
                .ThenInclude(a => a!.Vendor)
            .ToListAsync();
    }

    public async Task<List<ContractVendorAllocation>> GetVendorAllocationsAsync(
        int vendorID,
        int productID,
        int outletID)
    {
        var now = DateTime.Now;

        return await _context.ContractVendorAllocations
            .Include(a => a.Contract)
            .Include(a => a.Vendor)
            .Where(a =>
                a.VendorID == vendorID &&
                a.Contract != null &&
                a.Contract.ProductID == productID &&
                a.Contract.OutletID == outletID &&
                a.Contract.Status == "Active" &&
                a.Contract.StartDate <= now &&
                a.Contract.EndDate >= now &&
                a.Status == "Active")
            .ToListAsync();
    }

    public async Task<Contract?> UpdateAsync(Contract contract)
    {
        var existing =
            await _context.Contracts
                .Include(c => c.VendorAllocations)
                .FirstOrDefaultAsync(
                    c => c.ContractID == contract.ContractID);

        if (existing == null)
            return null;

        existing.StartDate =
            contract.StartDate;

        existing.EndDate =
            contract.EndDate;

        existing.PaymentMethod =
            contract.PaymentMethod;

        existing.Status =
            contract.Status;

        existing.UsedQuantity =
            contract.UsedQuantity;

        foreach (var allocation in contract.VendorAllocations)
        {
            var existingAllocation =
                existing.VendorAllocations
                    .FirstOrDefault(
                        a => a.ContractVendorAllocationID ==
                             allocation.ContractVendorAllocationID);

            if (existingAllocation == null)
                continue;

            existingAllocation.AllocationPercentage =
                allocation.AllocationPercentage;

            existingAllocation.AllocatedQuantity =
                allocation.AllocatedQuantity;

            existingAllocation.UsedQuantity =
                allocation.UsedQuantity;

            existingAllocation.Status =
                allocation.Status;
        }

        await _context.SaveChangesAsync();

        return existing;
    }

    public async Task<bool> DeleteAsync(int contractID)
    {
        var contract =
            await _context.Contracts
                .FirstOrDefaultAsync(
                    c => c.ContractID == contractID);

        if (contract == null)
            return false;

        _context.Contracts.Remove(contract);

        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<List<ContractVendorAllocation>> GetVendorAllocationsForAnalysisAsync(
        int vendorID,
        int productID,
        int outletID)
    {
        return await _context.ContractVendorAllocations
            .Include(a => a.Contract)
            .Include(a => a.Vendor)
            .Where(a =>
                a.VendorID == vendorID &&
                a.Contract != null &&
                a.Contract.ProductID == productID &&
                a.Contract.OutletID == outletID)
            .ToListAsync();
    }
}
