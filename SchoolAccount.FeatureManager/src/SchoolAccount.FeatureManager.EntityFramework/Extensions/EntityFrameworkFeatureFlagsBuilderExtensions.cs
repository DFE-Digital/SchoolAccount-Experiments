using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using SchoolAccount.FeatureManager.Core;
using SchoolAccount.FeatureManager.Core.Abstraction;
using SchoolAccount.FeatureManager.Core.Builders;
using SchoolAccount.FeatureManager.Core.Providers;
using SchoolAccount.FeatureManager.Core.Sources;
using SchoolAccount.FeatureManager.EntityFramework.Abstraction;
using SchoolAccount.FeatureManager.EntityFramework.Sources;
using SchoolAccount.FeatureManager.EntityFramework.Stores;

namespace SchoolAccount.FeatureManager.EntityFramework.Extensions;

public static class EntityFrameworkFeatureFlagsBuilderExtensions
{
    /// <summary>
    /// Adds the EF Core read provider and write store over an existing context type.
    /// Register the context itself (AddDbContext&lt;TContext&gt;) separately as usual.
    /// Pass <paramref name="cacheTtl"/> to cache the row fetch (the data, not the verdict).
    /// </summary>
    public static FeatureFlagsBuilder AddEntityFramework<TContext>(
        this FeatureFlagsBuilder builder,
        TimeSpan? cacheTtl = null)
        where TContext : DbContext, IFeatureFlagDbContext
    {
        if (cacheTtl is not null)
        {
            builder.Services.AddMemoryCache();
        }

        // read
        builder.Services.AddScoped<IFeatureProvider>(sp =>
        {
            IStoredFeatureSource source = new EntityFrameworkStoredFeatureSource<TContext>(
                sp.GetRequiredService<TContext>());
            var featureOptions = sp.GetRequiredService<IOptions<FeatureOptions>>();

            if (cacheTtl.HasValue)
            {
                source = new CachingStoredFeatureSource(featureOptions, source, sp.GetRequiredService<IMemoryCache>(),
                    cacheTtl.Value);
            }

            return new StoredFeatureProvider(source);
        });

        // write
        builder.Services.AddScoped<IFeatureRuleStore>(sp =>
            new EntityFrameworkFeatureStore<TContext>(sp.GetRequiredService<TContext>()));
        builder.Services.AddScoped<IFeatureStore>(sp => sp.GetRequiredService<IFeatureRuleStore>());

        return builder;
    }
}