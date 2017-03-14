using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace MGM.Helpers
{
    public class LockerCollection<TKey> where TKey : struct
    {//todo: low timer for cleaning up
        private readonly object _lock = new object();

        private readonly ConcurrentDictionary<TKey, object> _lockers = new ConcurrentDictionary<TKey, object>();

        public object GetOrCreateLocker(TKey key)
        {
            object result;
            // ReSharper disable once InconsistentlySynchronizedField This is fast check
            if (_lockers.TryGetValue(key, out result))
            {
                return result;
            }

            lock (_lock)
            {
                if (!_lockers.ContainsKey(key))
                    if(!_lockers.TryAdd(key, new object()))throw new InvalidOperationException("TryAdd=false");
                return _lockers[key];
            }
        }

        public void FreeLocker(TKey key)
        {
            lock (_lock)
            {
                object val;
                if (!_lockers.TryRemove(key, out val))
                throw new InvalidOperationException("TryRemove = false");
            }
        }
    }
}