using AwesomeAssertions;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using SchoolAccount.FeatureManager.Core.Context;
using SchoolAccount.FeatureManager.Core.Providers;

namespace SchoolAccount.FeatureManager.Core.Tests.Caching;

public class CachingFeatureProviderTests
{
    private readonly IOptions<FeatureOptions> _options = Options.Create(new FeatureOptions());
    
    [Fact]
    public async Task Ensure_that_if_caching_is_enabled_it_actually_caches_value_instead_of_reevaluating()
    {
        // Arrange
        var inner = new FakeProvider(FeatureDecision.Enabled);
        
        using var cache = new MemoryCache(new MemoryCacheOptions());
        var caching = new CachingFeatureProvider(_options, inner, cache, TimeSpan.FromMinutes(5));
        
        var fature = new FeatureContext("A", TestServices.Empty, TargetingContext.Empty);

        // Act
        await caching.EvaluateAsync(fature);
        await caching.EvaluateAsync(fature);
        await caching.EvaluateAsync(fature);
        
        // Assert
        inner.CallCount.Should().Be(1);
    }

    [Fact]
    public async Task Ensure_different_features_are_cached_independently()
    {
        // Arrange
        var inner = new FakeProvider(FeatureDecision.Enabled);
        
        using var cache = new MemoryCache(new MemoryCacheOptions());
        var caching = new CachingFeatureProvider(_options, inner, cache, TimeSpan.FromMinutes(5));

        var featureA = new FeatureContext("A", TestServices.Empty, TargetingContext.Empty);
        var featureB = new FeatureContext("B", TestServices.Empty, TargetingContext.Empty);

        // Act
        await caching.EvaluateAsync(featureA);
        await caching.EvaluateAsync(featureB);
        await caching.EvaluateAsync(featureB);
        await caching.EvaluateAsync(featureA);
        await caching.EvaluateAsync(featureA);
        await caching.EvaluateAsync(featureA);
        await caching.EvaluateAsync(featureB);

        // Assert
        inner.CallCount.Should().Be(2);
    }

    // TTL expiry itself is time-dependent and deliberately not covered here (a sleep-based
    // test would be slow and slightly flaky); the "within TTL" and "different keys" cases
    // above cover the behavior that matters for correctness.
}
