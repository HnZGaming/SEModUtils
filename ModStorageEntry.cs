using System;
using System.Xml.Serialization;
using HNZ.Utils.Logging;
using VRage.Game.Components;

namespace HNZ.Utils
{
    public sealed class ModStorageEntry
    {
        static readonly Logger Log = LoggerManager.Create(nameof(ModStorageEntry));

        // todo maybe Guid just works with the XML parser
        [XmlIgnore]
        public Guid KeyGuid { get; private set; }

        [XmlAttribute]
        public string Key
        {
            get { return KeyGuid.ToString(); }
            set { KeyGuid = Guid.Parse(value); }
        }

        [XmlAttribute]
        public string Value;

        public ModStorageEntry()
        {
        }

        public ModStorageEntry(Guid keyGuid, string value)
        {
            KeyGuid = keyGuid;
            Value = value;
        }

        public bool Test(MyModStorageComponentBase storage)
        {
            if (storage == null) return false;

            string v;
            return storage.TryGetValue(KeyGuid, out v) && v == Value;
        }

        public override string ToString()
        {
            return $"{nameof(Value)}: {Value}, {nameof(KeyGuid)}: {KeyGuid}";
        }
    }
}