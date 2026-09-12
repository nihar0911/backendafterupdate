using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using VendorManagementprojApplication.Contracts.Persistence;
using VendorManagementprojDomain.Entities;
using VendorManagementprojPersistence.Data;

namespace VendorManagementprojPersistence.Repositories;

public class NotificationRepository : INotificationRepository
{
    private readonly VendorManagementDbContext _context;

    public NotificationRepository(VendorManagementDbContext context)
    {
        _context = context;
    }

    public async Task<List<Notification>> GetByUserIdAsync(int userId)
    {
        return await _context.Notifications
            .Where(n => n.UserID == userId)
            .OrderByDescending(n => n.CreatedDate)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<Notification?> GetByIdAsync(int notificationId)
    {
        return await _context.Notifications
            .FirstOrDefaultAsync(n => n.NotificationID == notificationId);
    }

    public async Task<Notification> AddAsync(Notification notification)
    {
        _context.Notifications.Add(notification);
        await _context.SaveChangesAsync();
        return notification;
    }

    public async Task UpdateAsync(Notification notification)
    {
        _context.Notifications.Update(notification);
        await _context.SaveChangesAsync();
    }

    public async Task<int> GetUnreadCountByUserIdAsync(int userId)
    {
        return await _context.Notifications
            .CountAsync(n => n.UserID == userId && !n.IsRead);
    }

    public async Task MarkAllAsReadByUserIdAsync(int userId)
    {
        await _context.Notifications
            .Where(n => n.UserID == userId && !n.IsRead)
            .ExecuteUpdateAsync(s => s.SetProperty(n => n.IsRead, true));
    }

    public async Task ClearAllByUserIdAsync(int userId)
    {
        await _context.Notifications
            .Where(n => n.UserID == userId)
            .ExecuteDeleteAsync();
    }
}
