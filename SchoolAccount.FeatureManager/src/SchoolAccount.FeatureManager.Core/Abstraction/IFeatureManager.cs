namespace SchoolAccount.FeatureManager.Core.Abstraction;

public interface IFeatureManager : IFeatureProviderManager
{
    IReadOnlyCollection<FeatureSourceDescriptor> Sources { get; }

    Task SetAsync(string featureName, bool enabled, CancellationToken cancellationToken = default);
    Task SetAsync(string sourceName, string featureName, bool enabled, CancellationToken cancellationToken = default);

    Task SetRuleAsync(string featureName, FeatureRule rule, CancellationToken cancellationToken = default);
    Task SetRuleAsync(string sourceName, string featureName, FeatureRule rule, CancellationToken cancellationToken = default);

    Task RemoveAsync(string featureName, CancellationToken cancellationToken = default);
    Task RemoveAsync(string sourceName, string featureName, CancellationToken cancellationToken = default);
}