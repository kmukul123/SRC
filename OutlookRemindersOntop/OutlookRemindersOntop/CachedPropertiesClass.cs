using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Concurrent;

namespace OutlookRemindersOntop
{
    class CachedPropertiesClass
    {
        private ConcurrentDictionary<string, object> cache = new ConcurrentDictionary<string, object>(); 

        public T GetCachedValue<T>([CallerMemberName]string caller = "") where T : class
        {
            if (string.IsNullOrEmpty(caller)) {
                Logger.Error("Null caller in GetCachedValue");
                return null;
            }
            object cachedValue;
            if (cache.TryGetValue(caller, out cachedValue))
            {
                return cachedValue as T;
            } else
            {
                return null;
            }
        }
        public bool GetCachedValue<T>(out T cachedValue, [CallerMemberName]string caller = "") /*where T : class*/
        {
            if (string.IsNullOrEmpty(caller))
            {
                Logger.Error("Null caller in GetCachedValue");
                cachedValue = default(T);
                return false;
            }
            object cacheObj;
            if (cache.TryGetValue(caller, out cacheObj))
            {
                cachedValue = (T)cacheObj;
                return true;
            }
            else
            {
                cachedValue = default(T);
                return false;
            }
        }


        public void SetCachedValue<T>(T value, [CallerMemberName]string caller = "")
        {
            if (string.IsNullOrEmpty(caller))
            {
                Logger.Error("Null caller in GetCachedValue");
                return;
            }
            cache.AddOrUpdate(caller,value, (key, existingVal) =>
            {
                Logger.Debug($"Updated value for {caller}");
                return value;
            });
        }
    }
}
