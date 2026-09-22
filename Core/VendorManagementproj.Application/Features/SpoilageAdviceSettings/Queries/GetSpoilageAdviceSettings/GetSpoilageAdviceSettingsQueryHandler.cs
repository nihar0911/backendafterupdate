using System.Threading;
using System.Threading.Tasks;
using MediatR;
using VendorManagementprojApplication.Contracts.Persistence;
using VendorManagementprojApplication.DTOs;

namespace VendorManagementprojApplication.Features.SpoilageAdviceSettings.Queries.GetSpoilageAdviceSettings;

public class GetSpoilageAdviceSettingsQueryHandler : IRequestHandler<GetSpoilageAdviceSettingsQuery, GetSpoilageAdviceSettingsResponse>
{
    private readonly ISpoilageAdviceSettingsRepository _repository;

    public GetSpoilageAdviceSettingsQueryHandler(ISpoilageAdviceSettingsRepository repository)
    {
        _repository = repository;
    }

    public async Task<GetSpoilageAdviceSettingsResponse> Handle(GetSpoilageAdviceSettingsQuery request, CancellationToken cancellationToken)
    {
        var settings = await _repository.GetSettingsAsync(cancellationToken);

        return new GetSpoilageAdviceSettingsResponse
        {
            Settings = new SpoilageAdviceSettingsDto
            {
                Id = settings.Id,
                RecentDeliveriesCount = settings.RecentDeliveriesCount,
                TrendTolerancePercentage = settings.TrendTolerancePercentage,
                HighWeightedSpoilageThreshold = settings.HighWeightedSpoilageThreshold,
                HighRecentSpoilageThreshold = settings.HighRecentSpoilageThreshold,
                HighMaximumSpoilageThreshold = settings.HighMaximumSpoilageThreshold,
                MediumWeightedSpoilageThreshold = settings.MediumWeightedSpoilageThreshold,
                LowWeightedSpoilageThreshold = settings.LowWeightedSpoilageThreshold,
                UpdatedAt = settings.UpdatedAt
            }
        };
    }
}
