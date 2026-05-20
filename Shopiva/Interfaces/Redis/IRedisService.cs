namespace Shopiva.Interfaces.Redis
{
    public interface IRedisService
    {
        Task<Result<bool>> SetAsync<T>(string key, T value, TimeSpan? expiry = null);
        Task<Result<T?>> GetAsync<T>(string key);
        Task<Result<bool>> DeleteAsync(string key);
    }
}
