using System;
using System.Collections.Concurrent;
using System.IO;
using Sandbox.ModAPI;
using VRage;

namespace HNZ.Utils.Communications
{
    public sealed class ProtobufModule
    {
        static readonly Logger Log = new Logger(typeof(ProtobufModule));
        readonly ushort _messageHandlerId;
        readonly IProtobufListener _listener;
        readonly ConcurrentQueue<byte[]> _messages;

        public ProtobufModule(ushort handlerId, IProtobufListener listener)
        {
            _messageHandlerId = handlerId;
            _listener = listener;
            _messages = new ConcurrentQueue<byte[]>();
        }

        public event Action<Command> OnChatCommandSentFromClient;

        public void Initialize()
        {
            MyAPIGateway.Multiplayer.RegisterSecureMessageHandler(_messageHandlerId, OnProtobufMessageReceived);
        }

        public void Close()
        {
            MyAPIGateway.Multiplayer.UnregisterSecureMessageHandler(_messageHandlerId, OnProtobufMessageReceived);
        }

        public void SendChatCommandToServer(Command command)
        {
            using (var stream = new ByteStream(1024, true))
            using (var binaryWriter = new BinaryWriter(stream))
            {
                binaryWriter.Write((byte)1);
                binaryWriter.WriteProtobuf(command);
                MyAPIGateway.Multiplayer.SendMessageToServer(_messageHandlerId, stream.Data);
            }
        }

        public void SendDataToOthers(byte loadId, byte[] load)
        {
            if (loadId == 1)
            {
                throw new InvalidOperationException("Load ID 1 is reserved for chat commands");
            }

            using (var stream = new ByteStream(1024, true))
            using (var binaryWriter = new BinaryWriter(stream))
            {
                binaryWriter.Write(loadId);
                binaryWriter.Write(load);
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
                if (loadId == 1)
                {
                    var command = binaryReader.ReadProtobuf<Command>();
                    OnChatCommandSentFromClient?.Invoke(command);
                    return;
                }

                if (_listener.TryProcessProtobuf(loadId, binaryReader))
                {
                    return;
                }

                throw new InvalidOperationException($"Invalid load ID: {loadId}");
            }
        }
    }
}