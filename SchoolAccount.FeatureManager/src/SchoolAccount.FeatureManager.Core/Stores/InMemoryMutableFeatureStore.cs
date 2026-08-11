using System.Collections.Concurrent;
using SchoolAccount.FeatureManager.Core.Abstraction;

namespace SchoolAccount.FeatureManager.Core.Stores;

public sealed class InMemoryMutableFeatureStore : IStoredFeatureSource, IFeatureStore
{
    private readonly ConcurrentDictionary<string, bool> _flags = new(StringComparer.OrdinalIgnoreCase);

    public async Task<StoredFeature?> GetAsync(string featureName, CancellationToken cancellationToken = default)
    {
        return _flags.TryGetValue(featureName, out var enabled)
            ? new StoredFeature(enabled, Rule: null)
            : null;
    }

    public Task SetAsync(string featureName, bool enabled, CancellationToken cancellationToken = default)
    {
        _flags[featureName] = enabled;
        return Task.CompletedTask;
    }

    public Task RemoveAsync(string featureName, CancellationToken cancellationToken = default)
    {
        _flags.TryRemove(featureName, out _);
        return Task.CompletedTask;
    }
}