using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using SchoolAccount.FeatureManager.Core.Abstraction;
using SchoolAccount.FeatureManager.Core.Context;

namespace SchoolAccount.FeatureManager.Core;

/// <summary>
/// Default <see cref="IFeatureManager"/>. Resolves the named write target via keyed DI at
/// call time, so within one scope repeated calls to the same source reuse the same
/// scoped instance (e.g. the same <c>DbContext</c>).
/// </summary>
public class FeatureManager(
    IOptions<FeatureOptions> featureOptions,
    IEnumerable<FeatureSourceDescriptor> sources, 
    IServiceProvider services,
    IEnumerable<IFeatureProvider> providers,
    ITargetingContextAccessor targetingAccessor) 
    : IFeatureManager
{
    private readonly FeatureOptions _options = featureOptions.Value;
    
    public IReadOnlyCollection<IFeatureProvider> Providers { get; } = 
        providers as IFeatureProvider[] ?? providers.ToArray();

    public IReadOnlyCollection<FeatureSourceDescriptor> Sources { get; } =
        sources as FeatureSourceDescriptor[] ?? sources.ToArray();
 
    public Task SetAsync(string featureName, bool enabled, CancellationToken cancellationToken = default)
    {
        return ExecuteAsync(
            ResolveImplicitSourceNames(), 
            name => ResolveStore(name).SetAsync(featureName, enabled, cancellationToken));
    }

    public async Task SetAsync(string sourceName, string featureName, bool enabled, CancellationToken cancellationToken = default)
    {
        await ResolveStore(sourceName)
            .SetAsync(featureName, enabled, cancellationToken);
    }

    public Task SetRuleAsync(string featureName, FeatureRule rule, CancellationToken cancellationToken = default)
    {
        return ExecuteAsync(
            ResolveImplicitSourceNames(true),
            name => ResolveRuleStore(name)
                .SetRuleAsync(featureName, rule, cancellationToken));
    }

    public async Task SetRuleAsync(string sourceName, string featureName, FeatureRule rule, CancellationToken cancellationToken = default)
    {
        await ResolveRuleStore(sourceName)
            .SetRuleAsync(featureName, rule, cancellationToken);
    }

    public Task RemoveAsync(string featureName, CancellationToken cancellationToken = default)
    {
        return ExecuteAsync(
            ResolveImplicitSourceNames(), 
            name => ResolveStore(name)
                .RemoveAsync(featureName, cancellationToken));
    }

    public async Task RemoveAsync(string sourceName, string featureName, CancellationToken cancellationToken = default)
    {
        await ResolveStore(sourceName)
            .RemoveAsync(featureName, cancellationToken);
    }
    
    public async Task<bool> IsEnabledAsync(string featureName, CancellationToken cancellationToken = default)
    {
        var targeting = await targetingAccessor.GetContextAsync(cancellationToken);
        return await EvaluateCoreAsync(featureName, targeting, cancellationToken);
    }

    public Task<bool> IsEnabledAsync(string featureName, TargetingContext targeting, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(targeting);
        return EvaluateCoreAsync(featureName, targeting, cancellationToken);
    }

    private async Task<bool> EvaluateCoreAsync(string featureName, TargetingContext targeting, CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(featureName);

        var context = new FeatureContext(featureName, services, targeting);

        foreach (var provider in Providers)
        {
            var decision = await provider.EvaluateAsync(context, cancellationToken);
            
            if (decision != FeatureDecision.Unknown)
            {
                return decision == FeatureDecision.Enabled; // first definite opinion wins
            }
        }

        return false;
    }

    private IFeatureStore ResolveStore(string sourceName)
    {
        return services.GetKeyedService<IFeatureStore>(sourceName)
               ?? throw new InvalidOperationException(
                   $"No writable feature source named '{sourceName}'. Registered sources: {Describe()}.");
    }

    private IFeatureRuleStore ResolveRuleStore(string sourceName)
    {
        return services.GetKeyedService<IFeatureRuleStore>(sourceName)
               ?? throw new InvalidOperationException(
                   $"Source '{sourceName}' does not support rule-based targeting, or is not registered. " +
                   $"Rule-capable sources: {Describe(rulesOnly: true)}.");
    }

    /// <summary>
    /// The ordered list of source names a no-<c>sourceName</c> call should try. An array,
    /// not a lazy sequence: the count is needed up front to decide whether the call is
    /// even ambiguous (and to build the error message) before anything is attempted, so
    /// deferring evaluation wouldn't save any real work here.
    /// </summary>
    private string[] ResolveImplicitSourceNames(bool requireRuleSupport = false)
    {
        var source = requireRuleSupport 
            ? Sources.Where(s => s.SupportsRules) 
            : Sources;
        var names = source
            .Select(s => s.Name)
            .ToArray();

        return names.Length switch
        {
            0 => throw new InvalidOperationException(requireRuleSupport
                ? $"No rule-capable feature sources are registered. Registered sources: {Describe()}."
                : "No writable feature sources are registered."),
            > 1 when _options.StrictSourceControl => throw new InvalidOperationException(
                $"Multiple feature sources are registered ({Describe(requireRuleSupport)}); use the sourceName " +
                "overload to pick one, or set FeatureOptions.StrictSourceControl = false to try them in " +
                "registration order until one succeeds."),
            _ => names
        };
    }
 
    /// <summary>
    /// Tries <paramref name="sourceNames"/> in order, returning as soon as one succeeds.
    /// With exactly one candidate this just awaits it directly — no fallback exists, so a
    /// failure propagates as-is. With more than one (only reachable when
    /// <see cref="FeatureOptions.StrictSourceControl"/> is <c>false</c>), a failure moves
    /// on to the next candidate; cancellation is never treated as a per-source failure, it
    /// propagates immediately. If every candidate fails, all failures are reported
    /// together via <see cref="AggregateException"/>.
    /// </summary>
    private static async Task ExecuteAsync(string[] sourceNames, Func<string, Task> attempt)
    {
        if (sourceNames.Length == 1)
        {
            await attempt(sourceNames[0]);
            return;
        }
 
        List<Exception>? failures = null;
        foreach (var name in sourceNames)
        {
            try
            {
                await attempt(name);
                return;
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                (failures ??= []).Add(ex);
            }
        }
 
        throw new AggregateException(
            $"All {sourceNames.Length} candidate feature sources failed: {string.Join(", ", sourceNames)}.",
            failures!);
    }
 
    private string Describe(bool rulesOnly = false)
    {
        return string.Join(", ", Sources.Where(s => !rulesOnly || s.SupportsRules).Select(s => s.Name));
    }
}