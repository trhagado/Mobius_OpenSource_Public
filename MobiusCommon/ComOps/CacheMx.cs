using LazyCache;
//using LazyCache.Providers;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
//using Microsoft.Extensions.Caching.Memory;

namespace Mobius.ComOps
{
  /// <summary>
  /// Generic cache class wrapper for LazyCache
  /// </summary>
  /// <typeparam name="T"></typeparam>

  public class CacheMx<T>
  {

    public static int HitCount = 0;
    public static int MissCount = 0;
    public static int AddCount = 0;
    public static int ReplaceCount = 0;

    public static IAppCache Cache
    {
      get
      {
        if (_cache == null)
          _cache = new CachingService();

        return _cache;
      }
    }
    private static IAppCache _cache;


    //static MemoryCacheEntryOptions EntryOptions = new MemoryCacheEntryOptions()
    //{
    //  SlidingExpiration = new TimeSpan(0, 5, 0)
    //  //Priority = CacheItemPriority.NeverRemove
    //};

    /// <summary>
    /// Add to cache
    /// </summary>
    /// <param name="key"></param>
    /// <param name="value"></param>

    public static void Add(
      string key,
      T value)
    {
      if (Contains(key)) // if already exists then remove and readd
      {
        Cache.Remove(key);
        ReplaceCount++;
      }

      else AddCount++;

      Cache.Add<T>(key, value, new TimeSpan(0, 5, 0)); // five minutes, new TimeSpan(,); DefaultCachePolicy.DefaultCacheDurationSeconds = 60 * 3
      return;
    }

    public static T Get(string key)
    {
      T value;

      if (TryGetValue(key, out value))
        return value;

      else throw new Exception("Cache key not found: " + key);
    }

    public static bool TryGetValue(
      string key,
      out T value)
    {
      value = Cache.Get<T>(key);

      if (value != null)
      {
        HitCount++;
        return true;
      }
      else
      {
        MissCount++;
        return false;
      }
    }

    public static bool Contains(string key)
    {
      if (Cache.Get<string>(key) != null)
        return true;

      else return false;
    }
  }
}