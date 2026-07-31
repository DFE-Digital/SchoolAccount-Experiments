using Azure.Data.AppConfiguration;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using SchoolAccount.FeatureManager.Azure.Providers;
using SchoolAccount.FeatureManager.Core.Abstraction;
using SchoolAccount.FeatureManager.Core.DependencyInjection;
using SchoolAccount.FeatureManager.Core.Providers;

namespace SchoolAccount.FeatureManager.Azure.Extensions;

public static class FeatureFlagsBuilderExtensions
{
    public static FeatureFlagsBuilder AddAzureAppConfiguration(
        this FeatureFlagsBuilder builder,
        AzureFeatureOptions options)
    {
        builder.Services.AddSingleton(_ => new ConfigurationClient(options.ConnectionString));

        if (options.CacheTimeout is not null)
        {
            builder.Services.AddMemoryCache();
        }

        builder.Services.AddScoped<IFeatureProvider>(sp =>
        {
            IFeatureProvider provider = new AzureAppConfigurationFeatureProvider(
                sp.GetRequiredService<ConfigurationClient>(), options.Label);

            if (options.CacheTimeout is { } ttl)
            {
                provider = new CachingFeatureProvider(provider, sp.GetRequiredService<IMemoryCache>(), ttl);
            }

            return provider;
        });

        return builder;
    }
}