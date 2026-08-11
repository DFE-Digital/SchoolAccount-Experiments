namespace SchoolAccount.FeatureManager.Core.Abstraction;

public interface IFeatureProvider
{
    Task<FeatureDecision> EvaluateAsync(FeatureContext context, CancellationToken cancellationToken = default);
}
