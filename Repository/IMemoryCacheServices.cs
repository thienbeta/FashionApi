using System;
using System.Threading.Tasks;

namespace FashionApi.Repository
{
    public interface IMemoryCacheServices
    {
        Task<T> GetOrCreateAsync<T>(string key, Func<Task<T>> factory, TimeSpan? absoluteExpireTime = null);
        void Remove(string key);
    }
}