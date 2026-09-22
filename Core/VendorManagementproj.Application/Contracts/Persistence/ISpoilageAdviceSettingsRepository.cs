using System.Threading;
using System.Threading.Tasks;
using VendorManagementprojDomain.Entities;

namespace VendorManagementprojApplication.Contracts.Persistence;

public interface ISpoilageAdviceSettingsRepository
{
    Task<SpoilageAdviceSettings> GetSettingsAsync(CancellationToken cancellationToken = default);

    Task<SpoilageAdviceSettings> UpdateSettingsAsync(SpoilageAdviceSettings settings, CancellationToken cancellationToken = default);
}
