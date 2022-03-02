using System.Collections.Generic;

namespace HNZ.Utils.Logging
{
    public static class LoggerManager
    {
        static readonly List<Logger> _loggers;

        static LoggerManager()
        {
            _loggers = new List<Logger>();
        }

        public static string Prefix { get; private set; }

        public static void SetPrefix(string prefix)
        {
            Prefix = prefix;

            foreach (var logger in _loggers)
            {
                logger.Prefix = prefix;
            }
        }

        public static void SetLogConfig(ICollection<LogConfig> configs)
        {
            foreach (var logger in _loggers)
            foreach (var config in configs)
            {
                logger.SetConfig(config);
            }
        }

        public static Logger Create(string name)
        {
            var logger = new Logger
            {
                Prefix = Prefix,
                Name = name,
            };

            _loggers.Add(logger);
            return logger;
        }
    }
}