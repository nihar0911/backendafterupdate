using VendorManagementproj.Domain.Entities;

namespace VendorManagementproj.Application.Contracts.Persistence;

public interface IVendorRepository
{
    Task<List<Vendor>> GetAllAsync();

    Task<Vendor?> GetByIdAsync(int vendorID);

    Task<Vendor> AddAsync(Vendor vendor);

    Task<Vendor?> UpdateAsync(
        int vendorID,
        Vendor vendor);

    Task<bool> DeleteAsync(int vendorID);
}