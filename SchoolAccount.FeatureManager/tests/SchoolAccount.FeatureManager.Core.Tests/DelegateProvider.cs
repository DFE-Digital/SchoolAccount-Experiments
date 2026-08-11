using SchoolAccount.FeatureManager.Core.Abstraction;

namespace SchoolAccount.FeatureManager.Core.Tests;

/// <summary>
/// A provider whose decision is computed from the context, for targeting-aware tests.
/// </summary>
internal sealed class DelegateProvider(Func<FeatureContext, FeatureDecision> decide) : IFeatureProvider
{
    public async Task<FeatureDecision> EvaluateAsync(FeatureContext context,
        CancellationToken cancellationToken = default)
    {
        return decide(context);
    }
}