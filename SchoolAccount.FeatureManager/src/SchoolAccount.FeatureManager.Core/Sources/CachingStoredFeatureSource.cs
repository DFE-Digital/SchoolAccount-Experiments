using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using SchoolAccount.FeatureManager.Core.Abstraction;

namespace SchoolAccount.FeatureManager.Core.Sources;

/// <summary>
/// Caches the <em>data</em> a source returns (per feature name), not the decision.
/// This is the correct layer to cache once targeting exists: the stored feature is the
/// same for everyone, while the decision is per subject. Null results are cached too
/// (negative caching), so a missing feature does not hit the backend every time.
/// </summary>
public sealed class CachingStoredFeatureSource(
    IOptions<FeatureOptions> options,
    IStoredFeatureSource inner,
    IMemoryCache cache,
    TimeSpan ttl)
    : IStoredFeatureSource
{
    private readonly IStoredFeatureSource _inner = inner ?? throw new ArgumentNullException(nameof(inner));
    private readonly IMemoryCache _cache = cache ?? throw new ArgumentNullException(nameof(cache));
    private readonly string _keyPrefix = $"{options.Value.CacheStorePrefix}:{inner.GetType().FullName}:";

    public async Task<StoredFeature?> GetAsync(string featureName, CancellationToken cancellationToken = default)
    {
        var key = _keyPrefix + featureName;

        if (_cache.TryGetValue(key, out StoredFeature? cached))
        {
            return cached;
        }

        var value = await _inner.GetAsync(featureName, cancellationToken).ConfigureAwait(false);
        _cache.Set(key, value, ttl);
        return value;
    }
}