namespace SchoolAccount.FeatureManager.Core.Abstraction;

public interface IFeatureManager
{
    ValueTask<bool> IsEnabledAsync(string featureName, CancellationToken cancellationToken = default);
}