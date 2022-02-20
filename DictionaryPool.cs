using System.Collections.Concurrent;
using System.Collections.Generic;

namespace HNZ.Utils
{
    public static class DictionaryPool<K, V>
    {
        static readonly ConcurrentBag<Dictionary<K, V>> _pool;

        static DictionaryPool()
        {
            _pool = new ConcurrentBag<Dictionary<K, V>>();
        }

        public static Dictionary<K, V> Create()
        {
            Dictionary<K, V> list;
            if (_pool.TryTake(out list))
            {
                return list;
            }

            return new Dictionary<K, V>();
        }

        public static void Release(Dictionary<K, V> list)
        {
            list.Clear();
            _pool.Add(list);
        }
    }
}