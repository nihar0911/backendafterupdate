using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using VendorManagementprojApplication.Contracts.Persistence;
using VendorManagementprojApplication.DTOs;
using VendorManagementprojDomain.Entities;
using SpoilageAdviceSettingsEntity = VendorManagementprojDomain.Entities.SpoilageAdviceSettings;

namespace VendorManagementprojApplication.Features.SpoilageAdviceSettings.Commands.UpdateSpoilageAdviceSettings;

public class UpdateSpoilageAdviceSettingsCommandHandler : IRequestHandler<UpdateSpoilageAdviceSettingsCommand, UpdateSpoilageAdviceSettingsResponse>
{
    private readonly ISpoilageAdviceSettingsRepository _repository;

    public UpdateSpoilageAdviceSettingsCommandHandler(ISpoilageAdviceSettingsRepository repository)
    {
        _repository = repository;
    }

    public async Task<UpdateSpoilageAdviceSettingsResponse> Handle(UpdateSpoilageAdviceSettingsCommand request, CancellationToken cancellationToken)
    {
        // 1. Validation
        if (request.RecentDeliveriesCount <= 0)
        {
            throw new InvalidOperationException("Recent deliveries count must be greater than 0.");
        }

        if (request.TrendTolerancePercentage < 0)
        {
            throw new InvalidOperationException("Trend tolerance percentage must be 0 or greater.");
        }

        if (request.LowWeightedSpoilageThreshold < 0)
        {
            throw new InvalidOperationException("Low weighted spoilage threshold must be 0 or greater.");
        }

        if (request.MediumWeightedSpoilageThreshold < 0)
        {
            throw new InvalidOperationException("Medium weighted spoilage threshold must be 0 or greater.");
        }

        if (request.HighWeightedSpoilageThreshold < 0)
        {
            throw new InvalidOperationException("High weighted spoilage threshold must be 0 or greater.");
        }

        if (request.LowWeightedSpoilageThreshold > request.MediumWeightedSpoilageThreshold)
        {
            throw new InvalidOperationException("Low threshold cannot be greater than the medium threshold.");
        }

        if (request.MediumWeightedSpoilageThreshold > request.HighWeightedSpoilageThreshold)
        {
            throw new InvalidOperationException("Medium threshold cannot be greater than the high weighted threshold.");
        }

        if (request.HighRecentSpoilageThreshold < 0)
        {
            throw new InvalidOperationException("High recent spoilage threshold must be 0 or greater.");
        }

        if (request.HighMaximumSpoilageThreshold < 0)
        {
            throw new InvalidOperationException("High maximum spoilage threshold must be 0 or greater.");
        }

        // 2. Persist
        var entity = new SpoilageAdviceSettingsEntity
        {
            RecentDeliveriesCount = request.RecentDeliveriesCount,
            TrendTolerancePercentage = Math.Round(request.TrendTolerancePercentage, 2),
            HighWeightedSpoilageThreshold = Math.Round(request.HighWeightedSpoilageThreshold, 2),
            HighRecentSpoilageThreshold = Math.Round(request.HighRecentSpoilageThreshold, 2),
            HighMaximumSpoilageThreshold = Math.Round(request.HighMaximumSpoilageThreshold, 2),
            MediumWeightedSpoilageThreshold = Math.Round(request.MediumWeightedSpoilageThreshold, 2),
            LowWeightedSpoilageThreshold = Math.Round(request.LowWeightedSpoilageThreshold, 2)
        };

        var updated = await _repository.UpdateSettingsAsync(entity, cancellationToken);

        return new UpdateSpoilageAdviceSettingsResponse
        {
            Message = "Spoilage advice settings saved successfully.",
            Settings = new SpoilageAdviceSettingsDto
            {
                Id = updated.Id,
                RecentDeliveriesCount = updated.RecentDeliveriesCount,
                TrendTolerancePercentage = updated.TrendTolerancePercentage,
                HighWeightedSpoilageThreshold = updated.HighWeightedSpoilageThreshold,
                HighRecentSpoilageThreshold = updated.HighRecentSpoilageThreshold,
                HighMaximumSpoilageThreshold = updated.HighMaximumSpoilageThreshold,
                MediumWeightedSpoilageThreshold = updated.MediumWeightedSpoilageThreshold,
                LowWeightedSpoilageThreshold = updated.LowWeightedSpoilageThreshold,
                UpdatedAt = updated.UpdatedAt
            }
        };
    }
}
