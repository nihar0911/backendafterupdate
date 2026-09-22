using VendorManagementprojDomain.Entities;

namespace VendorManagementprojApplication.Contracts.Persistence;

public interface IOrganizationRepository
{
    Task<List<Organization>> GetAllAsync();
    Task<Organization?> GetByIdAsync(int id);
    Task<Organization?> GetByEmailAsync(string email);
    Task AddAsync(Organization organization);
    Task UpdateAsync(Organization organization);
    Task DeleteAsync(Organization organization);
}