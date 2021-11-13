﻿using System;
using VRage.Utils;

namespace HNZ.Utils
{
    public sealed class Logger
    {
        readonly string _name;

        public Logger(Type type)
        {
            _name = $"{type.Namespace}.{type.Name}";
        }

        public bool IsDebugEnabled { get; set; }

        string Format(object message)
        {
            return $"{_name}: {message}";
        }

        public void Info(object message)
        {
            MyLog.Default.Info(Format(message));
        }

        public void Error(Exception exception)
        {
            MyLog.Default.Error(Format($"{exception.Message}\n{exception.StackTrace}"));
        }

        public void Debug(object message)
        {
            if (!IsDebugEnabled) return;

            MyLog.Default.Info(Format(message));
        }

        public void Warn(object message)
        {
            MyLog.Default.Warning(Format(message));
        }
    }
}