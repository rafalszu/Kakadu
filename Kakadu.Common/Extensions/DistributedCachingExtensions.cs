using System;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Distributed;

[assembly: InternalsVisibleTo("Kakadu.ActionApi.Tests")]
namespace Kakadu.Common.Extensions
{
    public static class DistributedCachingExtensions
    {  
        public async static Task SetAsync<T>(this IDistributedCache distributedCache, string key, T value, DistributedCacheEntryOptions options, CancellationToken token = default(CancellationToken))  
        {  
            await distributedCache.SetAsync(key, value.ToByteArray(), options, token);  
        }  
  
        public async static Task<T> GetAsync<T>(this IDistributedCache distributedCache, string key, CancellationToken token = default(CancellationToken))
        {  
            var result = await distributedCache.GetAsync(key, token);  
            return result.FromByteArray<T>();  
        }

        public async static Task<T> GetOrAddAsync<T>(this IDistributedCache distributedCache, string key, Func<DistributedCacheEntryOptions, Task<T>> getDataDelegate, CancellationToken token = default(CancellationToken))
        {
            var cached = await distributedCache.GetAsync<T>(key, token);
            if(cached != null && !cached.Equals(default(T)))
                return cached;

            DistributedCacheEntryOptions options = new DistributedCacheEntryOptions();
            // nothing found, execute delegate
            var result = await getDataDelegate(options);

            // store it in cache
            await distributedCache.SetAsync<T>(key, result, options, token);

            return result;
        }
    }

    internal static class SerializationExtensions
    {  
        internal static byte[] ToByteArray(this object obj)  
        {  
            if (obj == null)  
                return null;  

            BinaryFormatter binaryFormatter = new BinaryFormatter();  
            using (MemoryStream memoryStream = new MemoryStream())  
            {  
                binaryFormatter.Serialize(memoryStream, obj);  
                return memoryStream.ToArray();  
            }  
        }  

        internal static T FromByteArray<T>(this byte[] byteArray)
        {  
            if (byteArray == null)  
                return default(T);  

            BinaryFormatter binaryFormatter = new BinaryFormatter();  
            using (MemoryStream memoryStream = new MemoryStream(byteArray))  
            {  
                return (T)binaryFormatter.Deserialize(memoryStream);  
            }  
        }
    }
}