using SchoolAccount.FeatureManager.Core.Abstraction;

namespace SchoolAccount.FeatureManager.Core.Providers;

public sealed class InMemoryFeatureProvider(IReadOnlyDictionary<string, bool> flags) : IFeatureProvider
{
    private readonly Dictionary<string, bool> _flags = new(flags, StringComparer.OrdinalIgnoreCase);

    public Task<FeatureDecision> EvaluateAsync(FeatureContext context, IServiceProvider serviceProvider,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var outcome = _flags.TryGetValue(context.FeatureName, out var enabled)
                ? (enabled ? FeatureDecision.Enabled : FeatureDecision.Disabled)
                : FeatureDecision.Unknown;

            return Task.FromResult(outcome);
        }
        catch (Exception exception)
        {
            return Task.FromException<FeatureDecision>(exception);
        }
    }
}