using System.Collections.Generic;
using System.Threading.Tasks;
using VendorManagementprojDomain.Entities;

namespace VendorManagementprojApplication.Contracts.Persistence;

public interface INotificationRepository
{
    Task<List<Notification>> GetByUserIdAsync(int userId);
    Task<Notification?> GetByIdAsync(int notificationId);
    Task<Notification> AddAsync(Notification notification);
    Task UpdateAsync(Notification notification);
    Task<int> GetUnreadCountByUserIdAsync(int userId);
}
