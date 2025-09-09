using FashionApi.Repository;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace FashionApi.Services
{
    public class MemoryCacheServices : IMemoryCacheServices
    {
        private readonly IMemoryCache _cache;
        private readonly ILogger<MemoryCacheServices> _logger;

        public MemoryCacheServices(IMemoryCache cache, ILogger<MemoryCacheServices> logger)
        {
            _cache = cache ?? throw new ArgumentNullException(nameof(cache));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<T> GetOrCreateAsync<T>(string key, Func<Task<T>> factory, TimeSpan? absoluteExpireTime = null)
        {
            if (string.IsNullOrEmpty(key))
            {
                _logger.LogWarning("Khóa cache không hợp lệ: {Key}", key);
                throw new ArgumentNullException(nameof(key), "Khóa cache không được null hoặc rỗng.");
            }

            if (factory == null)
            {
                _logger.LogWarning("Hàm factory không hợp lệ cho khóa: {Key}", key);
                throw new ArgumentNullException(nameof(factory), "Hàm factory không được null.");
            }

            _logger.LogDebug("Kiểm tra cache với khóa: {Key}", key);

            if (_cache.TryGetValue(key, out T value))
            {
                _logger.LogDebug("Lấy dữ liệu từ cache thành công: {Key}", key);
                return value;
            }

            _logger.LogInformation("Không tìm thấy cache, đang gọi factory cho khóa: {Key}", key);
            try
            {
                value = await factory();
                if (value == null)
                {
                    _logger.LogWarning("Factory trả về null cho khóa: {Key}", key);
                    return default;
                }

                var options = new MemoryCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = absoluteExpireTime ?? TimeSpan.FromMinutes(10)
                };

                _cache.Set(key, value, options);
                _logger.LogInformation("Lưu dữ liệu vào cache thành công: {Key}, Hết hạn: {Expiration}",
                    key, options.AbsoluteExpirationRelativeToNow?.TotalSeconds);

                return value;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy hoặc tạo cache cho khóa: {Key}, StackTrace: {StackTrace}", key, ex.StackTrace);
                throw;
            }
        }

        public void Remove(string key)
        {
            if (string.IsNullOrEmpty(key))
            {
                _logger.LogWarning("Khóa cache không hợp lệ khi xóa: {Key}", key);
                throw new ArgumentNullException(nameof(key), "Khóa cache không được null hoặc rỗng.");
            }

            _logger.LogDebug("Xóa cache với khóa: {Key}", key);
            _cache.Remove(key);
            _logger.LogInformation("Đã xóa cache thành công: {Key}", key);
        }
    }
}