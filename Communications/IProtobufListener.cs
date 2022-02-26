using System.IO;

namespace HNZ.Utils.Communications
{
    public interface IProtobufListener
    {
        bool TryProcessProtobuf(byte loadId, BinaryReader binaryReader);
    }
}