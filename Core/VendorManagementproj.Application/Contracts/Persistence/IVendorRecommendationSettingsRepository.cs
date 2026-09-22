using System.Threading;
using System.Threading.Tasks;
using VendorManagementproj.Domain.Entities;

namespace VendorManagementproj.Application.Contracts.Persistence;

public interface IVendorRecommendationSettingsRepository
{
    Task<VendorRecommendationSettings> GetSettingsAsync(CancellationToken cancellationToken = default);

    Task<VendorRecommendationSettings> UpdateSettingsAsync(VendorRecommendationSettings settings, CancellationToken cancellationToken = default);
}
