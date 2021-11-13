using System;
using System.Collections.Generic;

namespace HNZ.Utils
{
    public sealed class BoundaryObserver<T>
    {
        readonly HashSet<T> _currentElements;
        readonly HashSet<T> _lastElements;
        readonly HashSet<T> _processElements;

        public BoundaryObserver()
        {
            _currentElements = new HashSet<T>();
            _lastElements = new HashSet<T>();
            _processElements = new HashSet<T>();
        }

        public event Action<T> OnEntered;
        public event Action<T> OnExited;

        public void Close()
        {
            _currentElements.Clear();
            _lastElements.Clear();
            _processElements.Clear();
        }

        public void Add(T element)
        {
            _currentElements.Add(element);
        }

        public void Remove(T element)
        {
            _currentElements.Remove(element);
        }

        public void Update()
        {
            _processElements.Clear();
            _processElements.UnionWith(_lastElements);
            _processElements.ExceptWith(_currentElements);
            foreach (var exitedElements in _processElements)
            {
                OnExited?.Invoke(exitedElements);
            }

            _processElements.Clear();
            _processElements.UnionWith(_currentElements);
            _processElements.ExceptWith(_lastElements);
            foreach (var enteredElements in _processElements)
            {
                OnEntered?.Invoke(enteredElements);
            }

            _lastElements.Clear();
            _lastElements.UnionWith(_currentElements);

            _currentElements.Clear();
            _processElements.Clear();
        }

        public IEnumerable<T> GetLastElements()
        {
            return _lastElements;
        }
    }
}