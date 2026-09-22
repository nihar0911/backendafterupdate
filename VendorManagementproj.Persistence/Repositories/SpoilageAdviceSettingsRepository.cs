using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using VendorManagementprojApplication.Contracts.Persistence;
using VendorManagementprojDomain.Entities;
using VendorManagementprojPersistence.Data;

namespace VendorManagementprojPersistence.Repositories;

public class SpoilageAdviceSettingsRepository : ISpoilageAdviceSettingsRepository
{
    private readonly VendorManagementDbContext _context;

    public SpoilageAdviceSettingsRepository(VendorManagementDbContext context)
    {
        _context = context;
    }

    public async Task<SpoilageAdviceSettings> GetSettingsAsync(CancellationToken cancellationToken = default)
    {
        var settings = await _context.SpoilageAdviceSettings
            .FirstOrDefaultAsync(cancellationToken);

        if (settings == null)
        {
            settings = new SpoilageAdviceSettings
            {
                RecentDeliveriesCount = 5,
                TrendTolerancePercentage = 1.0m,
                HighWeightedSpoilageThreshold = 5.0m,
                HighRecentSpoilageThreshold = 6.0m,
                HighMaximumSpoilageThreshold = 10.0m,
                MediumWeightedSpoilageThreshold = 2.0m,
                LowWeightedSpoilageThreshold = 2.0m,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            await _context.SpoilageAdviceSettings.AddAsync(settings, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);
        }

        return settings;
    }

    public async Task<SpoilageAdviceSettings> UpdateSettingsAsync(SpoilageAdviceSettings settings, CancellationToken cancellationToken = default)
    {
        var existing = await _context.SpoilageAdviceSettings
            .FirstOrDefaultAsync(cancellationToken);

        if (existing == null)
        {
            settings.Id = 0;
            settings.CreatedAt = DateTime.UtcNow;
            settings.UpdatedAt = DateTime.UtcNow;
            await _context.SpoilageAdviceSettings.AddAsync(settings, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);
            return settings;
        }

        existing.RecentDeliveriesCount = settings.RecentDeliveriesCount;
        existing.TrendTolerancePercentage = settings.TrendTolerancePercentage;
        existing.HighWeightedSpoilageThreshold = settings.HighWeightedSpoilageThreshold;
        existing.HighRecentSpoilageThreshold = settings.HighRecentSpoilageThreshold;
        existing.HighMaximumSpoilageThreshold = settings.HighMaximumSpoilageThreshold;
        existing.MediumWeightedSpoilageThreshold = settings.MediumWeightedSpoilageThreshold;
        existing.LowWeightedSpoilageThreshold = settings.LowWeightedSpoilageThreshold;
        existing.UpdatedAt = DateTime.UtcNow;

        _context.SpoilageAdviceSettings.Update(existing);
        await _context.SaveChangesAsync(cancellationToken);

        return existing;
    }
}
