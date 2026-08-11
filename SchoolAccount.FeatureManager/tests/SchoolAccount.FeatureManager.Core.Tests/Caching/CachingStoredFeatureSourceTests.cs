using AwesomeAssertions;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using SchoolAccount.FeatureManager.Core.Sources;

namespace SchoolAccount.FeatureManager.Core.Tests.Caching;

public class CachingStoredFeatureSourceTests
{
    private readonly IOptions<FeatureOptions> _options = Options.Create(new FeatureOptions());

    [Fact]
    public async Task Ensure_all_items_are_cached_between_called_against_ttl()
    {
        // Arrange
        var source = new FakeStoredFeatureSource(new Dictionary<string, StoredFeature?>
        {
            ["A"] = new(true, null)
        });
        
        using var cache = new MemoryCache(new MemoryCacheOptions());
        var caching = new CachingStoredFeatureSource(_options, source, cache, TimeSpan.FromMinutes(5));

        // Act
        await caching.GetAsync("A");
        await caching.GetAsync("A");
        await caching.GetAsync("A");

        // Assert
        source.CallCount.Should().Be(1);
    }

    [Fact]
    public async Task Ensure_missing_feature_request_is_also_cached()
    {
        // Arrange
        var source = new FakeStoredFeatureSource(new Dictionary<string, StoredFeature?>());
        
        using var cache = new MemoryCache(new MemoryCacheOptions());
        var caching = new CachingStoredFeatureSource(_options, source, cache, TimeSpan.FromMinutes(5));

        // Act
        await caching.GetAsync("Missing");
        await caching.GetAsync("Missing");

        // Assert
        source.CallCount.Should().Be(1);
    }

    [Fact]
    public async Task Ensure_different_features_are_cached_independently()
    {
        // Arrange
        var source = new FakeStoredFeatureSource(new Dictionary<string, StoredFeature?>
        {
            ["A"] = new (true, null),
            ["B"] = new (false, null)
        });
        
        using var cache = new MemoryCache(new MemoryCacheOptions());
        var caching = new CachingStoredFeatureSource(_options, source, cache, TimeSpan.FromMinutes(5));

        // Act
        await caching.GetAsync("A");
        await caching.GetAsync("B");

        // Assert
        source.CallCount.Should().Be(2);
    }
}
