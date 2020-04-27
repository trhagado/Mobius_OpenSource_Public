using LazyCache;
using LazyCache.Providers;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;

namespace Mobius.Caching
{
    public class CacheMx
    {

    private static IAppCache Cache
    {
      get
      {
        if (_cache == null)
          _cache = new CachingService();

        return _cache;
      }
    }

    static MemoryCacheEntryOptions options = new MemoryCacheEntryOptions()
    {
      SlidingExpiration = new TimeSpan(0, 5, 0)
      //Priority = CacheItemPriority.NeverRemove
    };

    private static IAppCache _cache;

    public static void Add(
      string key,
      string value)
    {
      Cache.Add<string>(key, value, options); // five minutes, new TimeSpan(,); DefaultCachePolicy.DefaultCacheDurationSeconds = 60 * 3
    }

    public static string Get(string key)
    {
      string value;

      if (TryGetValue(key, out value))
        return value;

      else throw new Exception("Cache key not found: " + key);
    }

    public static bool TryGetValue(
      string key,
      out string value)
    {
      value = _cache.Get<string>(key);

      if (value != null) return true;
      else return false;
    }
  }
}
