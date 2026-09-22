using System.Threading;
using System.Threading.Tasks;
using VendorManagementproj.Domain.Entities;

namespace VendorManagementproj.Application.Contracts.Persistence;

public interface ISpoilageAdviceSettingsRepository
{
    Task<SpoilageAdviceSettings> GetSettingsAsync(CancellationToken cancellationToken = default);

    Task<SpoilageAdviceSettings> UpdateSettingsAsync(SpoilageAdviceSettings settings, CancellationToken cancellationToken = default);
}
