using System.Collections.Generic;
using VRage.Utils;

namespace HNZ.Utils.Logging
{
    public static class LoggerManager
    {
        static readonly List<Logger> _loggers;
        static readonly List<LogConfig> _configs;

        static LoggerManager()
        {
            _loggers = new List<Logger>();
            _configs = new List<LogConfig>();
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

        public static void SetConfigs(IEnumerable<LogConfig> configs)
        {
            _configs.Clear();
            _configs.AddRange(configs);

            MyLog.Default.Info($"LoggerManager({Prefix}).SetConfigs({configs.SeqToString()})");

            foreach (var logger in _loggers)
            {
                logger.SetConfig(_configs);
            }
        }

        public static Logger Create(string name)
        {
            MyLog.Default.Info($"LoggerManager({Prefix}).Create({name})");

            var logger = new Logger
            {
                Prefix = Prefix,
                Name = name,
            };

            logger.SetConfig(_configs);

            _loggers.Add(logger);
            return logger;
        }
    }
}