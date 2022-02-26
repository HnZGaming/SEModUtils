using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using Sandbox.ModAPI;
using VRage;

namespace HNZ.Utils.Communications
{
    public sealed class ProtobufModule
    {
        static readonly Logger Log = new Logger(typeof(ProtobufModule));
        readonly ushort _messageHandlerId;
        readonly List<IProtobufListener> _listeners;
        readonly ConcurrentQueue<byte[]> _messages;

        public ProtobufModule(ushort handlerId)
        {
            _messageHandlerId = handlerId;
            _listeners = new List<IProtobufListener>();
            _messages = new ConcurrentQueue<byte[]>();
        }

        public void Initialize()
        {
            MyAPIGateway.Multiplayer.RegisterSecureMessageHandler(_messageHandlerId, OnProtobufMessageReceived);
        }

        public void Close()
        {
            MyAPIGateway.Multiplayer.UnregisterSecureMessageHandler(_messageHandlerId, OnProtobufMessageReceived);
            _listeners.Clear();
        }

        public void AddListener(IProtobufListener listener)
        {
            _listeners.Add(listener);
        }

        public void RemoveListener(IProtobufListener listener)
        {
            _listeners.Remove(listener);
        }

        public void SendDataToServer(byte loadId, byte[] load)
        {
            using (var stream = new ByteStream(1024, true))
            using (var binaryWriter = new BinaryWriter(stream))
            {
                binaryWriter.Write(loadId);
                binaryWriter.Write(load);
                MyAPIGateway.Multiplayer.SendMessageToServer(_messageHandlerId, stream.Data);
            }
        }

        public void SendDataToClients(byte loadId, byte[] load)
        {
            using (var stream = new ByteStream(1024, true))
            using (var binaryWriter = new BinaryWriter(stream))
            {
                binaryWriter.Write(loadId);
                binaryWriter.Write(load);

                if (MyAPIGateway.Session.IsServer && !MyAPIGateway.Utilities.IsDedicated)
                {
                    Log.Info("routing local server message to client");
                    var player = MyAPIGateway.Session.LocalHumanPlayer;
                    OnProtobufMessageReceived(_messageHandlerId, stream.Data, player.SteamUserId, true);
                    return;
                }

                MyAPIGateway.Multiplayer.SendMessageToOthers(_messageHandlerId, stream.Data);
            }
        }

        void OnProtobufMessageReceived(ushort id, byte[] load, ulong steamId, bool sentFromServer)
        {
            if (id == _messageHandlerId)
            {
                _messages.Enqueue(load);
            }
        }

        public void Update()
        {
            byte[] load;
            while (_messages.TryDequeue(out load))
            {
                ProcessMessage(load);
            }
        }

        void ProcessMessage(byte[] load)
        {
            using (var stream = new ByteStream(load, load.Length))
            using (var binaryReader = new BinaryReader(stream))
            {
                var loadId = binaryReader.ReadByte();
                Log.Info($"protobuf received id: {loadId}");
                foreach (var listener in _listeners)
                {
                    if (listener.TryProcessProtobuf(loadId, binaryReader))
                    {
                        return;
                    }
                }

                Log.Error($"Invalid load ID: {loadId}");
            }
        }
    }
}