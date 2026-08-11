using SchoolAccount.FeatureManager.Core.Context;

namespace SchoolAccount.FeatureManager.Core.Abstraction;

public interface IFeatureProviderManager
{
    /// <summary>Evaluates using the ambient <see cref="TargetingContext"/> (from the accessor).</summary>
    Task<bool> IsEnabledAsync(string featureName, CancellationToken cancellationToken = default);

    /// <summary>
    /// Evaluates against an explicit subject. Use this to answer "would this be on for
    /// user X / region Y" from a background job or an admin preview.
    /// </summary>
    Task<bool> IsEnabledAsync(string featureName, TargetingContext targeting, CancellationToken cancellationToken = default);
}