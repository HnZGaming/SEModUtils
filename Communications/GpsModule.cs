using System.Collections.Generic;
using System.IO;
using HNZ.Utils.Logging;
using Sandbox.ModAPI;
using VRage;
using VRage.Game.ModAPI;

namespace HNZ.Utils.Communications
{
    public sealed class GpsModule : IProtobufListener
    {
        static readonly Logger Log = LoggerManager.Create(nameof(GpsModule));

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
            //Log.Info($"gps outgoing: {src}");

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
            //Log.Info($"gps incoming: {src}");

            UpdateLocalGps(src);
            return true;
        }

        void UpdateLocalGps(GpsSource src)
        {
            IMyGps gps;
            if (!_gps.TryGetValue(src.Id, out gps))
            {
                gps = _gps[src.Id] = MyAPIGateway.Session.GPS.Create(src.Name, src.Description, src.Position, true, false);
                MyAPIGateway.Session.GPS.AddLocalGps(gps);
            }

            gps.Name = src.Name;
            gps.Description = src.Description;
            gps.Coords = src.Position;
            gps.GPSColor = src.Color;
        }
    }
}