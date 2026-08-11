using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using SchoolAccount.FeatureManager.Core.Abstraction;
using SchoolAccount.FeatureManager.Core.Builders;
using SchoolAccount.FeatureManager.Core.Context;

namespace SchoolAccount.FeatureManager.Core.Extensions;

public static class FeatureFlagsServiceCollectionExtensions
{
    public static FeatureFlagsBuilder AddFeatureFlags(
        this IServiceCollection services
    )
    {
        return services.AddFeatureFlags(null, null);
    }
    
    public static FeatureFlagsBuilder AddFeatureFlags(
        this IServiceCollection services,
        ConfigurationSection optionsSections
    )
    {
        return services.AddFeatureFlags(optionsSections, null);
    }
    
    public static FeatureFlagsBuilder AddFeatureFlags(
        this IServiceCollection services,
        Action<FeatureOptions>? configure
    )
    {
        return services.AddFeatureFlags(null, configure);
    }
    
    public static FeatureFlagsBuilder AddFeatureFlags(
        this IServiceCollection services,
        IConfigurationSection? optionsSections,
        Action<FeatureOptions>? configure)
    {
        ArgumentNullException.ThrowIfNull(services);

        if (configure is not null)
        {
            services.Configure(configure);
        }

        var options = services.AddOptions<FeatureOptions>();

        if (optionsSections is not null)
        {
            options.Bind(optionsSections);
        }
        
        services.TryAddScoped<IFeatureProviderManager, FeatureProviderProviderManager>();
        services.TryAddScoped<ITargetingContextAccessor, EmptyTargetingContextAccessor>();
        
        return new FeatureFlagsBuilder(services);
    }
}