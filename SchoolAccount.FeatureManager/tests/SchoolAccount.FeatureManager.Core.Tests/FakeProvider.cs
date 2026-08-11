using SchoolAccount.FeatureManager.Core.Abstraction;

namespace SchoolAccount.FeatureManager.Core.Tests;

/// <summary>A provider that always returns a fixed decision and counts how many times it was evaluated.</summary>
internal sealed class FakeProvider(FeatureDecision decision) : IFeatureProvider
{
    public int CallCount { get; private set; }

    public async Task<FeatureDecision> EvaluateAsync(FeatureContext context, CancellationToken cancellationToken = default)
    {
        CallCount++;
        return decision;
    }
}