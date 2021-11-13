using System.IO;
using Sandbox.ModAPI;

namespace HNZ.Utils
{
    public static class ProtobufUtils
    {
        public static T ReadProtobuf<T>(this BinaryReader self)
        {
            var length = self.ReadInt32();
            var load = self.ReadBytes(length);
            var content = MyAPIGateway.Utilities.SerializeFromBinary<T>(load);
            return content;
        }

        public static void WriteProtobuf<T>(this BinaryWriter self, T content)
        {
            var load = MyAPIGateway.Utilities.SerializeToBinary(content);
            self.Write(load.Length);
            self.Write(load);
        }
    }
}