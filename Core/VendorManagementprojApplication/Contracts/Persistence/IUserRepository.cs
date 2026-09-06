using VendorManagementprojDomain.Entities;

namespace VendorManagementprojApplication.Contracts.Persistence;

public interface IUserRepository
{
    Task<User?> GetByIdAsync(int userID);

    Task<User?> GetByEmailAsync(string email);

    Task<List<User>> GetAllAsync();

    Task<bool> EmailExistsAsync(string email);

    Task<User> AddAsync(User user);

    Task UpdateAsync(User user);

    Task<bool> DeleteAsync(int userID);

    Task<Role?> GetRoleByNameAsync(string roleName);
    
    Task<Role?> GetRoleByIdAsync(int roleID);

    Task<User?> GetPurchaseManagerByOutletIdAsync(int outletID);
}