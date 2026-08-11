using SchoolAccount.FeatureManager.Core.Abstraction;
using SchoolAccount.FeatureManager.Core.Context;

namespace SchoolAccount.FeatureManager.Core.Tests;

/// <summary>A targeting accessor that always returns a fixed context, for testing the ambient vs. explicit paths.</summary>
internal sealed class FixedTargetingContextAccessor(TargetingContext context) : ITargetingContextAccessor
{
    public ValueTask<TargetingContext> GetContextAsync(CancellationToken cancellationToken = default)
    {
        return new ValueTask<TargetingContext>(context);
    }
}