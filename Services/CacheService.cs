using CargoManagementProject.core.Interfaces;
using Microsoft.Extensions.Caching.Distributed;
using System.Text.Json;

namespace CargoManagement.Services
{
    public class CacheService : ICacheService
    {
        private readonly IDistributedCache _cache;
        public CacheService(IDistributedCache cache)
        {
            _cache = cache;
        }
        public async Task<T> GetAsync<T>(string key)
        {
            var cacheData = await _cache.GetAsync(key);
            if (cacheData == null)
            {
                return default;
            }
            return JsonSerializer.Deserialize<T>(cacheData);

        }

        public async Task RemoveAsync(string key)
        {
            await _cache.RemoveAsync(key);
        }

        public async Task SetAsync<T>(string key, T value, TimeSpan? absoluteExpireTime = null, TimeSpan? slidingExpireTime = null)
        {
            var options = new DistributedCacheEntryOptions();

            if (absoluteExpireTime.HasValue)
            {
                options.SetAbsoluteExpiration(absoluteExpireTime.Value);
            }

            if (slidingExpireTime.HasValue)
            {
                options.SetSlidingExpiration(slidingExpireTime.Value);
            }

            var serializedData = JsonSerializer.Serialize(value);
            await _cache.SetStringAsync(key, serializedData, options);
        }
    }
}
