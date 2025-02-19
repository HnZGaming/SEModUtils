using System;
using ProtoBuf;
using Sandbox.ModAPI;
using VRage.Game.ModAPI;

namespace HNZ.Utils.MES
{
    [ProtoContract]
    public sealed class NpcData
    {
        static readonly Guid Key = new Guid("29FFD684-13D7-4045-BF76-CD48BF80E36A"); // copied from mes

        [ProtoMember(4)]
        public string SpawnGroupName;

        [ProtoMember(7)]
        public string OriginalPrefabId;

        [ProtoMember(42)]
        public string Context; // arbitrary user data via MESApi

        public static bool TryGetNpcData(IMyCubeGrid grid, out NpcData npcData)
        {
            npcData = null;
            if (grid.Storage == null) return false;

            string value;
            if (!grid.Storage.TryGetValue(Key, out value)) return false;

            var bytes = Convert.FromBase64String(value);
            npcData = MyAPIGateway.Utilities.SerializeFromBinary<NpcData>(bytes);
            return true;
        }
    }
}