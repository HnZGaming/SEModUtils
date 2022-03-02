using System;
using VRage.Utils;

namespace HNZ.Utils.Logging
{
    public sealed class Logger
    {
        public string Prefix { get; set; } = "";
        public string Name { get; set; } = "";
        public MyLogSeverity Severity { get; set; } = MyLogSeverity.Info;

        public void SetConfig(LogConfig config)
        {
            if (Name.StartsWith(config.Prefix))
            {
                Severity = config.Severity;
            }
        }

        string Format(object message)
        {
            return $"{Prefix}.{Name}: {message}";
        }

        public void Log(MyLogSeverity level, object message)
        {
            if (Severity > level) return;
            MyLog.Default.Log(level, Format(message));
        }

        public void Info(object message)
        {
            if (Severity > MyLogSeverity.Info) return;
            MyLog.Default.Info(Format(message));
        }

        public void Error(Exception exception)
        {
            if (Severity > MyLogSeverity.Error) return;
            MyLog.Default.Error(Format($"{exception.Message}\n{exception.StackTrace}"));
        }

        public void Error(string message)
        {
            if (Severity > MyLogSeverity.Error) return;
            MyLog.Default.Error(Format($"{message}"));
        }

        public void Debug(object message)
        {
            if (Severity > MyLogSeverity.Debug) return;

            MyLog.Default.Info(Format(message));
        }

        public void Warn(object message)
        {
            if (Severity > MyLogSeverity.Warning) return;
            MyLog.Default.Warning(Format(message));
        }
    }
}