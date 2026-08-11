using SchoolAccount.FeatureManager.Core.Abstraction;

namespace SchoolAccount.FeatureManager.Core.Tests;

/// <summary>A stored-feature source backed by a dictionary, with a call counter for cache tests.</summary>
internal sealed class FakeStoredFeatureSource(Dictionary<string, StoredFeature?> values) : IStoredFeatureSource
{
    public int CallCount { get; private set; }

    public async Task<StoredFeature?> GetAsync(string featureName, CancellationToken cancellationToken = default)
    {
        CallCount++;
        return values.GetValueOrDefault(featureName);
    }
}