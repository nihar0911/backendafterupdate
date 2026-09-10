using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using VendorManagementprojApplication.Contracts.Persistence;
using VendorManagementprojDomain.Entities;
using VendorManagementprojPersistence.Data;

namespace VendorManagementprojPersistence.Repositories;

public class VendorRecommendationSettingsRepository : IVendorRecommendationSettingsRepository
{
    private readonly VendorManagementDbContext _context;

    public VendorRecommendationSettingsRepository(VendorManagementDbContext context)
    {
        _context = context;
    }

    public async Task<VendorRecommendationSettings> GetSettingsAsync(CancellationToken cancellationToken = default)
    {
        var settings = await _context.VendorRecommendationSettings
            .FirstOrDefaultAsync(cancellationToken);

        if (settings == null)
        {
            settings = new VendorRecommendationSettings
            {
                QualityWeight = 35.0m,
                DeliveryWeight = 25.0m,
                PriceWeight = 25.0m,
                ReliabilityWeight = 15.0m,
                ReliabilityPointsPerReview = 3.0m,
                NeutralScoreForNewVendors = 70.0m,
                BestQualityThreshold = 4.5m,
                FastestDeliveryThreshold = 4.5m,
                HighQualityRationaleThreshold = 4.0m,
                PrioritizeActiveContracts = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            await _context.VendorRecommendationSettings.AddAsync(settings, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);
        }

        return settings;
    }

    public async Task<VendorRecommendationSettings> UpdateSettingsAsync(VendorRecommendationSettings settings, CancellationToken cancellationToken = default)
    {
        var existing = await _context.VendorRecommendationSettings
            .FirstOrDefaultAsync(cancellationToken);

        if (existing == null)
        {
            settings.Id = 0;
            settings.CreatedAt = DateTime.UtcNow;
            settings.UpdatedAt = DateTime.UtcNow;
            await _context.VendorRecommendationSettings.AddAsync(settings, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);
            return settings;
        }

        existing.QualityWeight = settings.QualityWeight;
        existing.DeliveryWeight = settings.DeliveryWeight;
        existing.PriceWeight = settings.PriceWeight;
        existing.ReliabilityWeight = settings.ReliabilityWeight;
        existing.ReliabilityPointsPerReview = settings.ReliabilityPointsPerReview;
        existing.NeutralScoreForNewVendors = settings.NeutralScoreForNewVendors;
        existing.BestQualityThreshold = settings.BestQualityThreshold;
        existing.FastestDeliveryThreshold = settings.FastestDeliveryThreshold;
        existing.HighQualityRationaleThreshold = settings.HighQualityRationaleThreshold;
        existing.PrioritizeActiveContracts = settings.PrioritizeActiveContracts;
        existing.UpdatedAt = DateTime.UtcNow;

        _context.VendorRecommendationSettings.Update(existing);
        await _context.SaveChangesAsync(cancellationToken);

        return existing;
    }
}
