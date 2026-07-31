namespace SchoolAccount.FeatureManager.Core.Abstraction;

public interface IFeatureProvider
{
    Task<FeatureDecision> EvaluateAsync(FeatureContext context, IServiceProvider serviceProvider, CancellationToken cancellationToken = default);
}