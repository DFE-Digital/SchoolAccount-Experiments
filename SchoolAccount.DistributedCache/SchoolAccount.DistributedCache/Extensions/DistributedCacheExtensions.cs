using System.Text.Json;
using Microsoft.Extensions.Caching.Distributed;

namespace SchoolAccount.DistributedCache.Extensions;

public static class DistributedCacheExtensions
{
    public static async Task<T> GetOrCreateAsync<T>(this IDistributedCache cache, string key, Func<Task<T>> factory,
        DistributedCacheEntryOptions? options = null)
    {
        var cached = await cache.GetStringAsync(key);

        if (cached is not null)
        {
            return JsonSerializer.Deserialize<T>(cached) ?? throw new InvalidOperationException();
        }

        var value = await factory();
        
        options ??= new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(30)
        };
        await cache.SetStringAsync(key, JsonSerializer.Serialize(value), options);

        return value;
    }
}