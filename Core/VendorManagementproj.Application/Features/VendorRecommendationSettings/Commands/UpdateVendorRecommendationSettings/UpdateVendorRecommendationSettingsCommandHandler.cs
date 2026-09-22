using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using VendorManagementprojApplication.Contracts.Persistence;
using VendorManagementprojApplication.DTOs;
using VendorRecommendationSettingsEntity = VendorManagementprojDomain.Entities.VendorRecommendationSettings;

namespace VendorManagementprojApplication.Features.VendorRecommendationSettings.Commands.UpdateVendorRecommendationSettings;

public class UpdateVendorRecommendationSettingsCommandHandler : IRequestHandler<UpdateVendorRecommendationSettingsCommand, UpdateVendorRecommendationSettingsResponse>
{
    private readonly IVendorRecommendationSettingsRepository _repository;

    public UpdateVendorRecommendationSettingsCommandHandler(IVendorRecommendationSettingsRepository repository)
    {
        _repository = repository;
    }

    public async Task<UpdateVendorRecommendationSettingsResponse> Handle(UpdateVendorRecommendationSettingsCommand request, CancellationToken cancellationToken)
    {
        // 1. Validation
        if (request.QualityWeight < 0 || request.DeliveryWeight < 0 || request.PriceWeight < 0 || request.ReliabilityWeight < 0)
        {
            throw new InvalidOperationException("Scoring weights cannot be negative.");
        }

        var totalWeight = Math.Round(request.QualityWeight + request.DeliveryWeight + request.PriceWeight + request.ReliabilityWeight, 2);
        if (totalWeight != 100.00m)
        {
            throw new InvalidOperationException($"The sum of scoring weights (Quality + Delivery + Price + Reliability) must equal 100.00%. Current sum: {totalWeight}%.");
        }

        if (request.ReliabilityPointsPerReview <= 0)
        {
            throw new InvalidOperationException("Reliability points per review must be greater than 0.");
        }

        if (request.NeutralScoreForNewVendors < 0 || request.NeutralScoreForNewVendors > 100)
        {
            throw new InvalidOperationException("Neutral score for new vendors must be between 0 and 100.");
        }

        if (request.BestQualityThreshold < 1.0m || request.BestQualityThreshold > 5.0m)
        {
            throw new InvalidOperationException("Best Quality threshold must be between 1.0 and 5.0.");
        }

        if (request.FastestDeliveryThreshold < 1.0m || request.FastestDeliveryThreshold > 5.0m)
        {
            throw new InvalidOperationException("Fastest Delivery threshold must be between 1.0 and 5.0.");
        }

        if (request.HighQualityRationaleThreshold < 1.0m || request.HighQualityRationaleThreshold > 5.0m)
        {
            throw new InvalidOperationException("High Quality rationale threshold must be between 1.0 and 5.0.");
        }

        // 2. Persist
        var entity = new VendorRecommendationSettingsEntity
        {
            QualityWeight = Math.Round(request.QualityWeight, 2),
            DeliveryWeight = Math.Round(request.DeliveryWeight, 2),
            PriceWeight = Math.Round(request.PriceWeight, 2),
            ReliabilityWeight = Math.Round(request.ReliabilityWeight, 2),
            ReliabilityPointsPerReview = Math.Round(request.ReliabilityPointsPerReview, 2),
            NeutralScoreForNewVendors = Math.Round(request.NeutralScoreForNewVendors, 2),
            BestQualityThreshold = Math.Round(request.BestQualityThreshold, 2),
            FastestDeliveryThreshold = Math.Round(request.FastestDeliveryThreshold, 2),
            HighQualityRationaleThreshold = Math.Round(request.HighQualityRationaleThreshold, 2),
            PrioritizeActiveContracts = request.PrioritizeActiveContracts
        };

        var updated = await _repository.UpdateSettingsAsync(entity, cancellationToken);

        return new UpdateVendorRecommendationSettingsResponse
        {
            Message = "Vendor recommendation settings saved successfully.",
            Settings = new VendorRecommendationSettingsDto
            {
                Id = updated.Id,
                QualityWeight = updated.QualityWeight,
                DeliveryWeight = updated.DeliveryWeight,
                PriceWeight = updated.PriceWeight,
                ReliabilityWeight = updated.ReliabilityWeight,
                ReliabilityPointsPerReview = updated.ReliabilityPointsPerReview,
                NeutralScoreForNewVendors = updated.NeutralScoreForNewVendors,
                BestQualityThreshold = updated.BestQualityThreshold,
                FastestDeliveryThreshold = updated.FastestDeliveryThreshold,
                HighQualityRationaleThreshold = updated.HighQualityRationaleThreshold,
                PrioritizeActiveContracts = updated.PrioritizeActiveContracts,
                UpdatedAt = updated.UpdatedAt
            }
        };
    }
}
