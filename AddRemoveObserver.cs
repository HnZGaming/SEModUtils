using System;
using System.Collections.Concurrent;
using HNZ.Utils.Pools;

namespace HNZ.Utils
{
    public sealed class AddRemoveObserver<T>
    {
        readonly ConcurrentQueue<T> _addedElements;
        readonly ConcurrentQueue<T> _removedElements;

        public AddRemoveObserver()
        {
            _addedElements = new ConcurrentQueue<T>();
            _removedElements = new ConcurrentQueue<T>();
        }

        public event Action<T> OnAdded;
        public event Action<T> OnRemoved;

        public void Close()
        {
            _addedElements.DequeueAll();
            _removedElements.DequeueAll();
        }

        public void Add(T element)
        {
            _addedElements.Enqueue(element);
        }

        public void Remove(T element)
        {
            _removedElements.Enqueue(element);
        }

        public void Update()
        {
            var tmpRemovedElements = ListPool<T>.Get();
            var tmpAddedElements = ListPool<T>.Get();

            // minimum locking
            _removedElements.DequeueAll(tmpRemovedElements);
            _addedElements.DequeueAll(tmpAddedElements);

            foreach (var element in tmpRemovedElements)
            {
                OnRemoved?.Invoke(element);
            }

            foreach (var element in tmpAddedElements)
            {
                OnAdded?.Invoke(element);
            }

            ListPool<T>.Release(tmpRemovedElements);
            ListPool<T>.Release(tmpAddedElements);
        }
    }
}