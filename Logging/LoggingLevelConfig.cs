using System;
using System.Xml.Serialization;

namespace HNZ.Utils.Logging
{
    [Serializable]
    public sealed class LoggingLevelConfig
    {
        [XmlAttribute]
        public string Name;

        [XmlAttribute]
        public int Level;
    }
}