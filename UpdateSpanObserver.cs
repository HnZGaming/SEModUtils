using System;

namespace HNZ.Utils
{
    public sealed class UpdateSpanObserver
    {
        readonly TimeSpan _frequency;
        DateTime? _lastUpdateTime;

        public UpdateSpanObserver(TimeSpan frequency)
        {
            _frequency = frequency;
        }

        public bool Update()
        {
            var nowTime = DateTime.UtcNow;
            if (_lastUpdateTime != null)
            {
                var pastTime = nowTime - _lastUpdateTime.Value;
                if (pastTime < _frequency) return false;
            }

            _lastUpdateTime = nowTime;
            return true;
        }
    }
}