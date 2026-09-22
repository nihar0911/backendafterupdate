using VendorManagementprojDomain.Entities;

namespace VendorManagementprojApplication.Contracts.Persistence;

public interface IOutletRepository
{
    Task<List<Outlet>> GetAllAsync();
    Task<Outlet?> GetByIdAsync(int id);
    Task<List<Outlet>> GetByOrganizationIdAsync(int organizationId);
    Task<Outlet> AddAsync(Outlet outlet);
    Task<Outlet> UpdateAsync(Outlet outlet);
    Task<bool> DeleteAsync(int id);
}