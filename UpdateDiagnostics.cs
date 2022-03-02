using System;
using System.Diagnostics;
using HNZ.Utils.Logging;

namespace HNZ.Utils
{
    public sealed class UpdateDiagnostics
    {
        static readonly Logger Log = LoggerManager.Create(nameof(UpdateDiagnostics));
        readonly Action _onUpdateEnd;
        Stopwatch _stopwatch;

        public UpdateDiagnostics()
        {
            _onUpdateEnd = () => OnUpdateEnd();
        }

        public IDisposable Update()
        {
            _stopwatch = Stopwatch.StartNew();
            return new Disposable(_onUpdateEnd);
        }

        void OnUpdateEnd()
        {
            _stopwatch.Stop();
            var ms = _stopwatch.ElapsedMilliseconds();
            Log.Debug($"Frame: {ms:0.00}ms");
        }
    }
}