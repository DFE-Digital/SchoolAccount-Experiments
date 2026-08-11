using SchoolAccount.FeatureManager.Core.Abstraction;

namespace SchoolAccount.FeatureManager.Core.Context;

/// <summary>Default accessor. Returns an empty (anonymous) context.</summary>
public sealed class EmptyTargetingContextAccessor : ITargetingContextAccessor
{
    public ValueTask<TargetingContext> GetContextAsync(CancellationToken cancellationToken = default)
        => new(TargetingContext.Empty);
}