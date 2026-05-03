using StackExchange.Redis;
using System.Text.Json;

namespace WebAPI.Services
{
    public class RedisCacheService : IRedisCacheService
    {
        private readonly IConnectionMultiplexer _redis;
        private readonly IDatabase _database;

        public RedisCacheService(IConnectionMultiplexer redis)
        {
            _redis = redis;
            _database = _redis.GetDatabase();
        }

        public async Task SetAsync<T>(string key, T value, TimeSpan? ttl = null)
        {
            var json = JsonSerializer.Serialize(value);
            await _database.StringSetAsync(key, json, (Expiration)ttl);
        }

        public async Task<T?> GetAsync<T>(string key)
        {
            var json = await _database.StringGetAsync(key);
            if (json.IsNullOrEmpty)
            {
                return default;
            }
            return JsonSerializer.Deserialize<T>(json.ToString());
        }
        public async Task RemoveAsync(string key)
        {
            await _database.KeyDeleteAsync(key);

        }
    }
}