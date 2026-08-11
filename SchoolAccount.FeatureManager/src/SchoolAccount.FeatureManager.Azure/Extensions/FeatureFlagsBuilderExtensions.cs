using Azure.Data.AppConfiguration;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using SchoolAccount.FeatureManager.Azure.Source;
using SchoolAccount.FeatureManager.Azure.Stores;
using SchoolAccount.FeatureManager.Core;
using SchoolAccount.FeatureManager.Core.Abstraction;
using SchoolAccount.FeatureManager.Core.Builders;
using SchoolAccount.FeatureManager.Core.Providers;
using SchoolAccount.FeatureManager.Core.Sources;

namespace SchoolAccount.FeatureManager.Azure.Extensions;

public static class FeatureFlagsBuilderExtensions
{
    public static FeatureFlagsBuilder AddAzureAppConfiguration(
        this FeatureFlagsBuilder builder,
        AzureFeatureOptions options)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(options.ConnectionString);

        builder.RegisterSource(options.SourceName, true);
        
        builder.Services.AddKeyedSingleton(options.SourceName, (_, _) => new ConfigurationClient(options.ConnectionString));
        builder.Services.AddKeyedSingleton(options.SourceName , (_, _) => Options.Create(options));

        if (options.CacheTimeout.HasValue)
        {
            builder.Services.AddMemoryCache();
        }

        builder.Services.AddScoped<IFeatureProvider>(sp =>
        {
            IStoredFeatureSource source =
                new AzureStoredFeatureSource(
                    sp.GetRequiredService<ConfigurationClient>(),
                    sp.GetRequiredService<IOptions<AzureFeatureOptions>>());

            if (options.CacheTimeout.HasValue)
            {
                source = new CachingStoredFeatureSource(
                    sp.GetRequiredService<IOptions<FeatureOptions>>(),
                    source,
                    sp.GetRequiredService<IMemoryCache>(),
                    options.CacheTimeout.Value);
            }

            return new StoredFeatureProvider(source);
        });

        builder.Services.AddKeyedScoped<IFeatureStore>(options.SourceName, (sp, _) =>
            new AzureFeatureStore(
                sp.GetRequiredService<ConfigurationClient>(),
                sp.GetRequiredService<IOptions<AzureFeatureOptions>>()));
        
        return builder;
    }
}