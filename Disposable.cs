using System;

namespace HNZ.Utils
{
    public struct Disposable : IDisposable
    {
        readonly Action _action;

        public Disposable(Action action)
        {
            _action = action;
        }

        public void Dispose()
        {
            _action?.Invoke();
        }
    }
}