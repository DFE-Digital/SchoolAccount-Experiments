namespace SchoolAccount.FeatureManager.Core.Abstraction;

/// <summary>A store that can also persist targeting rules.</summary>
public interface IFeatureRuleStore : IFeatureStore
{
    Task SetRuleAsync(string featureName, FeatureRule rule, CancellationToken cancellationToken = default);
}