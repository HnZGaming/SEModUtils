using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace HNZ.Utils
{
    public sealed class AddRemoveObserver<T>
    {
        readonly ConcurrentQueue<T> _addedElements;
        readonly ConcurrentQueue<T> _removedElements;
        readonly List<T> _tmpAddedElements;
        readonly List<T> _tmpRemovedElements;

        public AddRemoveObserver()
        {
            _addedElements = new ConcurrentQueue<T>();
            _removedElements = new ConcurrentQueue<T>();
            _tmpAddedElements = new List<T>();
            _tmpRemovedElements = new List<T>();
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
            try
            {
                // minimum locking
                _removedElements.DequeueAll(_tmpRemovedElements);
                _addedElements.DequeueAll(_tmpAddedElements);

                foreach (var element in _tmpRemovedElements)
                {
                    OnRemoved?.Invoke(element);
                }

                foreach (var element in _tmpAddedElements)
                {
                    OnAdded?.Invoke(element);
                }
            }
            finally
            {
                _tmpRemovedElements.Clear();
                _tmpAddedElements.Clear();
            }
        }
    }
}