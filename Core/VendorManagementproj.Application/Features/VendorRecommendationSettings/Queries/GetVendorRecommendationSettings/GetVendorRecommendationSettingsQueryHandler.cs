using System.Threading;
using System.Threading.Tasks;
using MediatR;
using VendorManagementprojApplication.Contracts.Persistence;
using VendorManagementprojApplication.DTOs;

namespace VendorManagementprojApplication.Features.VendorRecommendationSettings.Queries.GetVendorRecommendationSettings;

public class GetVendorRecommendationSettingsQueryHandler : IRequestHandler<GetVendorRecommendationSettingsQuery, GetVendorRecommendationSettingsResponse>
{
    private readonly IVendorRecommendationSettingsRepository _repository;

    public GetVendorRecommendationSettingsQueryHandler(IVendorRecommendationSettingsRepository repository)
    {
        _repository = repository;
    }

    public async Task<GetVendorRecommendationSettingsResponse> Handle(GetVendorRecommendationSettingsQuery request, CancellationToken cancellationToken)
    {
        var settings = await _repository.GetSettingsAsync(cancellationToken);

        return new GetVendorRecommendationSettingsResponse
        {
            Settings = new VendorRecommendationSettingsDto
            {
                Id = settings.Id,
                QualityWeight = settings.QualityWeight,
                DeliveryWeight = settings.DeliveryWeight,
                PriceWeight = settings.PriceWeight,
                ReliabilityWeight = settings.ReliabilityWeight,
                ReliabilityPointsPerReview = settings.ReliabilityPointsPerReview,
                NeutralScoreForNewVendors = settings.NeutralScoreForNewVendors,
                BestQualityThreshold = settings.BestQualityThreshold,
                FastestDeliveryThreshold = settings.FastestDeliveryThreshold,
                HighQualityRationaleThreshold = settings.HighQualityRationaleThreshold,
                PrioritizeActiveContracts = settings.PrioritizeActiveContracts,
                UpdatedAt = settings.UpdatedAt
            }
        };
    }
}
