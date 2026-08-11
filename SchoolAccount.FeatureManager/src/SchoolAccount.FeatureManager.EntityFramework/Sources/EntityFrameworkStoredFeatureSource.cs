using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using SchoolAccount.FeatureManager.Core;
using SchoolAccount.FeatureManager.Core.Abstraction;
using SchoolAccount.FeatureManager.Core.Providers;
using SchoolAccount.FeatureManager.EntityFramework.Abstraction;

namespace SchoolAccount.FeatureManager.EntityFramework.Sources;

/// <summary>
/// Reads the stored feature (bool + optional rule) from a context implementing
/// <see cref="IFeatureFlagDbContext"/>. Pure fetch, no evaluation — the
/// <see cref="StoredFeatureProvider"/> and cache decorator sit on top of it.
/// </summary>
public sealed class EntityFrameworkStoredFeatureSource<TContext>(TContext db) : IStoredFeatureSource
    where TContext : DbContext, IFeatureFlagDbContext
{
    public async Task<StoredFeature?> GetAsync(string featureName, CancellationToken cancellationToken = default)
    {
        var row = await db.FeatureFlags
            .AsNoTracking()
            .Where(f => f.Name == featureName)
            .Select(f => new { f.IsEnabled, f.RuleJson })
            .FirstOrDefaultAsync(cancellationToken);

        if (row is null)
        {
            return null;
        }

        var rule = !string.IsNullOrEmpty(row.RuleJson) ? JsonSerializer.Deserialize<FeatureRule>(row.RuleJson) : null;
        return new StoredFeature(row.IsEnabled, rule);
    }
}