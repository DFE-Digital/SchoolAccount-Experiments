using Microsoft.Extensions.DependencyInjection;
using SchoolAccount.FeatureManager.Core.Abstraction;
using SchoolAccount.FeatureManager.Core.DependencyInjection;

namespace SchoolAccount.FeatureManager.Core.Extensions;

public static class FeatureFlagsBuilderExtensions
{
    public static FeatureFlagsBuilder AddProvider<TProvider>(this FeatureFlagsBuilder builder)
        where TProvider : class, IFeatureProvider
    {
        builder.Services.AddScoped<IFeatureProvider, TProvider>();
        return builder;
    }

    public static FeatureFlagsBuilder AddProvider(this FeatureFlagsBuilder builder, IFeatureProvider provider)
    {
        ArgumentNullException.ThrowIfNull(provider);
        builder.Services.AddSingleton(provider);
        return builder;
    }

    public static FeatureFlagsBuilder AddInMemory(
        this FeatureFlagsBuilder builder,
        Action<IDictionary<string, bool>> configure)
    {
        ArgumentNullException.ThrowIfNull(configure);

        var flags = new Dictionary<string, bool>(StringComparer.OrdinalIgnoreCase);
        configure(flags);
        return builder.AddProvider(new InMemoryFeatureProvider(flags));
    }
}