using Microsoft.Extensions.DependencyInjection;
using SchoolAccount.FeatureManager.Core.Abstraction;

namespace SchoolAccount.FeatureManager.Core.Builders;

public sealed class FeatureFlagsBuilder(IServiceCollection services)
{
    public IServiceCollection Services { get; } = services ?? throw new ArgumentNullException(nameof(services));
    
    /// <summary>
    /// Registers a named, writable feature source so it shows up in
    /// <see cref="IFeatureManager.Sources"/> and can be targeted by name. Connector
    /// packages call this from their <c>Add...</c> extension alongside registering a
    /// keyed <see cref="IFeatureStore"/> (or <see cref="IFeatureRuleStore"/>) under the
    /// same <paramref name="name"/>. Throws if the name is already registered — this
    /// catches source-name collisions early, e.g. two EF contexts both defaulting to
    /// "EntityFramework" without a distinct <c>sourceName</c>.
    /// </summary>
    public FeatureFlagsBuilder RegisterSource(string name, bool supportsRules)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        var duplicate = Services.Any(d =>
            d.ServiceType == typeof(FeatureSourceDescriptor) &&
            d.ImplementationInstance is FeatureSourceDescriptor existing &&
            string.Equals(existing.Name, name, StringComparison.OrdinalIgnoreCase));

        if (duplicate)
        {
            throw new InvalidOperationException(
                $"A feature source named '{name}' is already registered. Pass a distinct sourceName.");
        }

        Services.AddSingleton(new FeatureSourceDescriptor(name, supportsRules));
        return this;
    }
}