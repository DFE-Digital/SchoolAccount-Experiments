namespace SchoolAccount.FeatureManager.Core.Abstraction;

/// <summary>
/// Fetches the stored representation of a feature. Deterministic per feature name and
/// independent of the caller, which is exactly why it (not the decision) is cacheable.
/// </summary>
public interface IStoredFeatureSource
{
    Task<StoredFeature?> GetAsync(string featureName, CancellationToken cancellationToken = default);
}