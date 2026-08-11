using SchoolAccount.FeatureManager.Core.Abstraction;
using SchoolAccount.FeatureManager.Core.Context;

namespace SchoolAccount.FeatureManager.Core.Providers;

/// <summary>
/// Adapts an <see cref="IStoredFeatureSource"/> into the read pipeline. Fetches the
/// stored feature (possibly cached) and, if it carries a rule, evaluates it against the
/// current <see cref="TargetingContext"/>. The verdict is computed per request; only the
/// fetch is cached.
/// </summary>
public sealed class StoredFeatureProvider(IStoredFeatureSource source) : IFeatureProvider
{
    private readonly IStoredFeatureSource _source = source ?? throw new ArgumentNullException(nameof(source));

    public async Task<FeatureDecision> EvaluateAsync(FeatureContext context, CancellationToken cancellationToken = default)
    {
        var stored = await _source.GetAsync(context.FeatureName, cancellationToken).ConfigureAwait(false);
        
        if (stored is null)
        {
            return FeatureDecision.Unknown;
        }

        if (stored.Rule is not null)
        {
            return RuleEvaluator.Evaluate(context.FeatureName, stored.Rule, context.Targeting);
        }

        return stored.Enabled switch
        {
            true => FeatureDecision.Enabled,
            false => FeatureDecision.Disabled,
            null => FeatureDecision.Unknown
        };
    }
}