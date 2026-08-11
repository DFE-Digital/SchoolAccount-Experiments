using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using SchoolAccount.FeatureManager.Core.Abstraction;

namespace SchoolAccount.FeatureManager.Core.Providers;

public sealed class CachingFeatureProvider(
    IOptions<FeatureOptions> options,
    IFeatureProvider provider, 
    IMemoryCache cache, 
    TimeSpan ttl) : IFeatureProvider
{
    private readonly IFeatureProvider _inner = provider ?? throw new ArgumentNullException(nameof(provider));
    private readonly IMemoryCache _cache = cache ?? throw new ArgumentNullException(nameof(cache));
    private readonly string _keyPrefix = $"{options.Value.CacheFeaturePrefix}:{provider.GetType().FullName}:";

    public async Task<FeatureDecision> EvaluateAsync(FeatureContext context, CancellationToken cancellationToken = default)
    {
        var key = _keyPrefix + context.FeatureName;

        if (_cache.TryGetValue(key, out FeatureDecision cached))
        {
            return cached;
        }

        var decision = await _inner.EvaluateAsync(context, cancellationToken);
        _cache.Set(key, decision, ttl);
        return decision;
    }
}