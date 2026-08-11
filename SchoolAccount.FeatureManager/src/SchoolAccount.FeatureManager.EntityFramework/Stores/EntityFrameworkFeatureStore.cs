using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using SchoolAccount.FeatureManager.Core;
using SchoolAccount.FeatureManager.Core.Abstraction;
using SchoolAccount.FeatureManager.EntityFramework.Abstraction;
using SchoolAccount.FeatureManager.EntityFramework.Entities;

namespace SchoolAccount.FeatureManager.EntityFramework.Stores;

/// <summary>Write side over an <see cref="IFeatureFlagDbContext"/> — plain values and rules.</summary>
public sealed class EntityFrameworkFeatureStore<TContext>(TContext db) : IFeatureRuleStore
    where TContext : DbContext, IFeatureFlagDbContext
{
    private readonly TContext _db = db ?? throw new ArgumentNullException(nameof(db));

    public async Task SetAsync(string featureName, bool enabled, CancellationToken cancellationToken = default)
    {
        var row = await Find(featureName, cancellationToken).ConfigureAwait(false);
        
        if (row is null)
        {
            _db.FeatureFlags.Add(new FeatureFlagEntity { Name = featureName, IsEnabled = enabled });
        }
        else
        {
            row.IsEnabled = enabled;
            row.RuleJson = null; // a plain set clears any prior rule
        }

        await _db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }

    public async Task SetRuleAsync(string featureName, FeatureRule rule, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(rule);
        
        var json = JsonSerializer.Serialize(rule);
        var row = await Find(featureName, cancellationToken).ConfigureAwait(false);
        
        if (row is null)
        {
            _db.FeatureFlags.Add(new FeatureFlagEntity { Name = featureName, IsEnabled = rule.Default ?? false, RuleJson = json });
        }
        else
        {
            row.RuleJson = json;
            row.IsEnabled = rule.Default ?? row.IsEnabled;
        }

        await _db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }

    public async Task RemoveAsync(string featureName, CancellationToken cancellationToken = default)
    {
        var row = await Find(featureName, cancellationToken).ConfigureAwait(false);
        
        if (row is null)
        {
            return;
        }

        _db.FeatureFlags.Remove(row);
        await _db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }

    private Task<FeatureFlagEntity?> Find(string name, CancellationToken ct)
        => _db.FeatureFlags.FirstOrDefaultAsync(f => f.Name == name, ct);
}
