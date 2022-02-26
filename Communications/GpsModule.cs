using System.Collections.Generic;
using System.IO;
using Sandbox.ModAPI;
using VRage;
using VRage.Game.ModAPI;

namespace HNZ.Utils.Communications
{
    public sealed class GpsModule : IProtobufListener
    {
        static readonly Logger Log = new Logger(typeof(GpsModule));

        readonly ProtobufModule _protobufModule;
        readonly byte _loadId;
        readonly Dictionary<long, IMyGps> _gps;

        public GpsModule(ProtobufModule protobufModule, byte loadId)
        {
            _protobufModule = protobufModule;
            _loadId = loadId;
            _gps = new Dictionary<long, IMyGps>();
        }

        public void Initialize()
        {
            _protobufModule.AddListener(this);
        }

        public void Close()
        {
            _protobufModule.RemoveListener(this);
        }

        public void ShowLocalGps(GpsSource src)
        {
            Log.Info($"gps outgoing: {src}");
            using (var stream = new ByteStream(1024))
            using (var writer = new BinaryWriter(stream))
            {
                writer.WriteProtobuf(src);
                _protobufModule.SendDataToClients(_loadId, stream.Data);
            }
        }

        public bool TryProcessProtobuf(byte loadId, BinaryReader binaryReader)
        {
            if (loadId != _loadId) return false;

            var src = binaryReader.ReadProtobuf<GpsSource>();
            Log.Info($"gps incoming: {src}");

            IMyGps oldGps;
            if (_gps.TryGetValue(src.Id, out oldGps))
            {
                MyAPIGateway.Session.GPS.RemoveLocalGps(oldGps);
                _gps.Remove(src.Id);
            }

            var gps = MyAPIGateway.Session.GPS.Create(src.Name, src.Description, src.Position, true, false);
            gps.GPSColor = src.Color;
            MyAPIGateway.Session.GPS.AddLocalGps(gps);

            _gps.Add(src.Id, gps);

            return true;
        }
    }
}