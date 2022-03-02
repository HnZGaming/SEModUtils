using System;
using System.Xml.Serialization;
using VRage.Utils;

namespace HNZ.Utils.Logging
{
    [Serializable]
    public sealed class LogConfig
    {
        [XmlAttribute]
        public MyLogSeverity Severity;

        [XmlAttribute]
        public string Prefix;
    }
}