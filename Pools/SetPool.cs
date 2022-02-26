using System.Collections.Concurrent;
using System.Collections.Generic;

namespace HNZ.Utils.Pools
{
    public sealed class SetPool<T>
    {
        static readonly ConcurrentBag<HashSet<T>> _pool;

        static SetPool()
        {
            _pool = new ConcurrentBag<HashSet<T>>();
        }

        public static HashSet<T> Create()
        {
            HashSet<T> set;
            if (_pool.TryTake(out set))
            {
                return set;
            }

            return new HashSet<T>();
        }

        public static void Release(HashSet<T> set)
        {
            set.Clear();
            _pool.Add(set);
        }
    }
}