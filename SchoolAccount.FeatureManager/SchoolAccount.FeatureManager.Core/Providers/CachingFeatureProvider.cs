using Microsoft.Extensions.Caching.Memory;
using SchoolAccount.FeatureManager.Core.Abstraction;

namespace SchoolAccount.FeatureManager.Core.Providers;


public sealed class CachingFeatureProvider(IFeatureProvider inner, IMemoryCache cache, TimeSpan ttl) : IFeatureProvider
{
    private readonly IFeatureProvider _inner = inner ?? throw new ArgumentNullException(nameof(inner));
    private readonly IMemoryCache _cache = cache ?? throw new ArgumentNullException(nameof(cache));
    private readonly string _keyPrefix = $"ff:{inner.GetType().FullName}:";

    public async Task<FeatureDecision> EvaluateAsync(FeatureContext context, IServiceProvider serviceProvider,
        CancellationToken cancellationToken = default)
    {
        var key = _keyPrefix + context.FeatureName;

        if (_cache.TryGetValue(key, out FeatureDecision cached))
        {
            return cached;
        }

        var decision = await _inner.EvaluateAsync(context, serviceProvider, cancellationToken);
        _cache.Set(key, decision, ttl);
        return decision;
    }
}