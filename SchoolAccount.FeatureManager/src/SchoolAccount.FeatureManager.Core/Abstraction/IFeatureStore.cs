namespace SchoolAccount.FeatureManager.Core.Abstraction;

/// <summary>
/// The write side, kept separate from the read pipeline. Not every provider can write
/// (a cookie is set client-side), so only backends that can persist implement this.
/// Writing sets the authoritative stored value; it does NOT bypass the pipeline on read
/// (the kill switch still wins).
/// </summary>
public interface IFeatureStore
{
    Task SetAsync(string featureName, bool enabled, CancellationToken cancellationToken = default);
    Task RemoveAsync(string featureName, CancellationToken cancellationToken = default);
}