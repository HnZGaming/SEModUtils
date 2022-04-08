using System.Collections.Concurrent;
using System.Collections.Generic;

namespace HNZ.Utils.Pools
{
    public static class ListPool<T>
    {
        static readonly ConcurrentBag<List<T>> _pool;

        static ListPool()
        {
            _pool = new ConcurrentBag<List<T>>();
        }

        public static List<T> Get()
        {
            List<T> list;
            if (_pool.TryTake(out list))
            {
                return list;
            }

            return new List<T>();
        }

        public static void Release(List<T> list)
        {
            list.Clear();
            _pool.Add(list);
        }
    }
}