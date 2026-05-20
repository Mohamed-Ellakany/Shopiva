using Newtonsoft.Json;
using Shopiva.Interfaces.Redis;
using StackExchange.Redis;

namespace Shopiva.Services
{
    public class RedisService : IRedisService
    {
        private readonly IDatabase _db;

        public RedisService(IConnectionMultiplexer redis)
        {
            _db = redis.GetDatabase();
        }


        public async Task<Result<bool>> DeleteAsync(string key)
        {
            bool result = await _db.KeyDeleteAsync(key);

            if (!result)
                return Result.Failure<bool>(UserErrors.ProblemOccured);

            return Result.Success(true);
        }

        public async Task<Result<T?>> GetAsync<T>(string key)
        {
            var json = await _db.StringGetAsync(key);

            if (json.IsNullOrEmpty) 
                return Result.Failure<T?>(UserErrors.ProblemOccured);

            return Result.Success(JsonConvert.DeserializeObject<T>(json!));
        }

        public async Task<Result<bool>> SetAsync<T>(string key, T value, TimeSpan? expiry = null)
        {
            var json = JsonConvert.SerializeObject(value);
            var result = await _db.StringSetAsync(key, json, expiry ?? TimeSpan.FromDays(30));

            if (!result)
                return Result.Failure<bool>(UserErrors.ProblemOccured);

            return Result.Success(true);
        }
    }
}
