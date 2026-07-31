using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace SchoolAccount.DistributedCache.Extensions;

public static class ServiceExtensions
{
    public static IServiceCollection AddDistributedCacheIfAbsent(this IServiceCollection services)
    {
        services.TryAddSingleton<IDistributedCache, MemoryDistributedCache>();
        services.AddMemoryCache();

        return services;
    }
}