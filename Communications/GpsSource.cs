using System;
using ProtoBuf;
using VRageMath;

namespace HNZ.Utils.Communications
{
    [Serializable]
    [ProtoContract]
    public sealed class GpsSource
    {
        [ProtoMember(1)]
        public long Id;

        [ProtoMember(2)]
        public string Name;

        [ProtoMember(3)]
        public string Description;

        [ProtoMember(4)]
        public Vector3D Position;

        [ProtoMember(5)]
        public Color Color;

        public override string ToString()
        {
            return $"{nameof(Id)}: {Id}, {nameof(Name)}: {Name}, {nameof(Description)}: {Description}, {nameof(Position)}: {Position}";
        }
    }
}