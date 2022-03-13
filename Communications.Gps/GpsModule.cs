﻿using System.Collections.Generic;
using System.IO;
using HNZ.Utils.Logging;
using Sandbox.ModAPI;
using VRage;
using VRage.Game.ModAPI;
using VRageMath;

namespace HNZ.Utils.Communications.Gps
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

        public void SendAddOrUpdateGps(GpsSource src, bool reliable = true, ulong? playerId = null)
        {
            //Log.Info($"gps outgoing: {src}");

            using (var stream = new ByteStream(1024))
            using (var writer = new BinaryWriter(stream))
            {
                writer.Write(true);
                writer.WriteProtobuf(src);
                _protobufModule.SendDataToClients(_loadId, stream.Data, reliable, playerId);
            }
        }

        public void SendRemoveGps(long gpsId, bool reliable = true, ulong? playerId = null)
        {
            using (var stream = new ByteStream(1024))
            using (var writer = new BinaryWriter(stream))
            {
                writer.Write(false);
                writer.Write(gpsId);
                _protobufModule.SendDataToClients(_loadId, stream.Data, reliable, playerId);
            }
        }

        bool IProtobufListener.TryProcessProtobuf(byte loadId, BinaryReader binaryReader)
        {
            if (loadId != _loadId) return false;

            if (binaryReader.ReadBoolean())
            {
                var src = binaryReader.ReadProtobuf<GpsSource>();
                AddOrUpdateGps(src);
            }
            else
            {
                var gpsId = binaryReader.ReadInt64();
                RemoveGps(gpsId);
            }

            return true;
        }

        void AddOrUpdateGps(IGpsEntity src)
        {
            var character = MyAPIGateway.Session.LocalHumanPlayer.Character;
            if (src.Radius >= 0 && Vector3D.Distance(character.GetPosition(), src.Position) > src.Radius)
            {
                RemoveGps(src.Id);
                return;
            }

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

        void RemoveGps(long gpsId)
        {
            IMyGps gps;
            if (_gps.TryGetValue(gpsId, out gps))
            {
                MyAPIGateway.Session.GPS.RemoveLocalGps(gps);
                _gps.Remove(gpsId);
            }
        }
    }
}