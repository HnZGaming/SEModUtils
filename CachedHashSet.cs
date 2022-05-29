using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace HNZ.Utils
{
    public sealed class CachedHashSet<T> : IEnumerable<T>
    {
        readonly HashSet<T> _hashset;
        readonly ConcurrentQueue<T> _removes;
        readonly ConcurrentQueue<T> _adds;

        public CachedHashSet()
        {
            _hashset = new HashSet<T>();
            _removes = new ConcurrentQueue<T>();
            _adds = new ConcurrentQueue<T>();
        }

        public IEnumerator<T> GetEnumerator()
        {
            lock (this)
            {
                return _hashset.GetEnumerator();
            }
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public void ApplyChanges()
        {
            lock (this)
            {
                _hashset.ExceptWith(_removes);
                _hashset.UnionWith(_adds);
                _removes.DequeueAll();
                _adds.DequeueAll();
            }
        }

        public void Remove(T element)
        {
            lock (this)
            {
                _removes.Enqueue(element);
            }
        }

        public void Add(T element)
        {
            lock (this)
            {
                _adds.Enqueue(element);
            }
        }
    }
}