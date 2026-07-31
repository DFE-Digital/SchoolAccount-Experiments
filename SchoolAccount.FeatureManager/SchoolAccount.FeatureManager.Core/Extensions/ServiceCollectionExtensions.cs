using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using SchoolAccount.FeatureManager.Core.Abstraction;
using SchoolAccount.FeatureManager.Core.DependencyInjection;

namespace SchoolAccount.FeatureManager.Core.Extensions;

public static class ServiceCollectionExtensions
{
    public static FeatureFlagsBuilder AddFeatureFlags(this IServiceCollection services,
        Action<FeatureOptions>? configure = null)
    {
        ArgumentNullException.ThrowIfNull(services);

        if (configure is not null)
        {
            services.Configure(configure);
        }

        services.AddOptions<FeatureOptions>();
        services.TryAddScoped<IFeatureManager, FeatureManager>();

        return new FeatureFlagsBuilder(services);
    }
}