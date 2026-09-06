using Microsoft.EntityFrameworkCore;
using VendorManagementprojApplication.Contracts.Persistence;
using VendorManagementprojDomain.Entities;
using VendorManagementprojPersistence.Data;

namespace VendorManagementprojPersistence.Repositories;

public class UserRepository : IUserRepository
{
    private readonly VendorManagementDbContext _context;

    public UserRepository(VendorManagementDbContext context)
    {
        _context = context;
    }

    public async Task<User?> GetByIdAsync(int userID)
    {
        return await _context.Users
            .Include(u => u.Role)
            .Include(u => u.Organization)
            .Include(u => u.Outlet)
            .FirstOrDefaultAsync(u => u.UserID == userID);
    }

    public async Task<User?> GetByEmailAsync(string email)
    {
        return await _context.Users
            .Include(u => u.Role)
            .Include(u => u.Organization)
            .Include(u => u.Outlet)
            .FirstOrDefaultAsync(u =>
                u.Email.ToLower() == email.ToLower());
    }

    public async Task<List<User>> GetAllAsync()
    {
        return await _context.Users
            .Include(u => u.Role)
            .Include(u => u.Organization)
            .Include(u => u.Outlet)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<bool> EmailExistsAsync(string email)
    {
        return await _context.Users
            .AnyAsync(u =>
                u.Email.ToLower() == email.ToLower());
    }

    public async Task<User> AddAsync(User user)
    {
        _context.Users.Add(user);

        await _context.SaveChangesAsync();

        return user;
    }

    public async Task UpdateAsync(User user)
    {
        _context.Users.Update(user);

        await _context.SaveChangesAsync();
    }

    public async Task<bool> DeleteAsync(int userID)
    {
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.UserID == userID);

        if (user == null)
            return false;

        _context.Users.Remove(user);

        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<Role?> GetRoleByNameAsync(string roleName)
    {
        return await _context.Roles
            .FirstOrDefaultAsync(r => r.RoleName.ToLower() == roleName.ToLower());
    }

    public async Task<Role?> GetRoleByIdAsync(int roleID)
    {
        return await _context.Roles
            .FirstOrDefaultAsync(r => r.RoleID == roleID);
    }

    public async Task<User?> GetPurchaseManagerByOutletIdAsync(int outletID)
    {
        return await _context.Users
            .Include(u => u.Role)
            .Include(u => u.Organization)
            .Include(u => u.Outlet)
            .FirstOrDefaultAsync(u =>
                u.OutletID == outletID &&
                u.Role != null &&
                u.Role.RoleName.ToLower() == "purchase manager");
    }
}