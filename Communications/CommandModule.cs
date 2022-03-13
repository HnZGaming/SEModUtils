using System;
using System.Collections.Concurrent;
using System.IO;
using System.Text.RegularExpressions;
using HNZ.Utils.Logging;
using Sandbox.ModAPI;
using VRage;

namespace HNZ.Utils.Communications
{
    public sealed class CommandModule : IProtobufListener
    {
        static readonly Logger Log = LoggerManager.Create(nameof(CommandModule));

        readonly ProtobufModule _protobuf;
        readonly byte _loadId;
        readonly ICommandListener _listener;
        readonly Regex _commandFormat;
        readonly ConcurrentQueue<Command> _messages;

        public CommandModule(ProtobufModule protobuf, byte loadId, string symbol, ICommandListener listener)
        {
            _protobuf = protobuf;
            _loadId = loadId;
            _listener = listener;
            _commandFormat = new Regex($@"^(?:/|!){symbol} (.+?)$");
            _messages = new ConcurrentQueue<Command>();
        }

        public void Initialize()
        {
            _protobuf.AddListener(this);
            MyAPIGateway.Utilities.MessageEntered += OnChatMessageEntered;
        }

        public void Close()
        {
            _protobuf.RemoveListener(this);
            MyAPIGateway.Utilities.MessageEntered -= OnChatMessageEntered;
        }

        bool IProtobufListener.TryProcessProtobuf(byte loadId, BinaryReader binaryReader)
        {
            if (loadId != _loadId) return false;

            var command = binaryReader.ReadProtobuf<Command>();
            _messages.Enqueue(command);
            return true;
        }

        void OnChatMessageEntered(string messageText, ref bool sendToOthers)
        {
            Command command;
            if (!TryParseCommand(messageText, out command)) return;
            sendToOthers = false;

            if (MyAPIGateway.Session.IsServer)
            {
                _messages.Enqueue(command);
            }
            else
            {
                // send client command to server
                using (var stream = new ByteStream(1024, true))
                using (var binaryWriter = new BinaryWriter(stream))
                {
                    binaryWriter.WriteProtobuf(command);
                    _protobuf.SendDataToServer(_loadId, stream.Data);
                }
            }
        }

        bool TryParseCommand(string messageText, out Command command)
        {
            command = new Command();
            var match = _commandFormat.Match(messageText);
            if (!match.Success) return false;

            var str = match.Groups[1].Value;
            var parts = str.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            var head = parts[0];
            var args = new string[parts.Length - 1];
            Array.Copy(parts, 1, args, 0, parts.Length - 1);

            var steamId = MyAPIGateway.Session.LocalHumanPlayer.SteamUserId;
            var playerId = MyAPIGateway.Session.LocalHumanPlayer.IdentityId;
            command = new Command(head, args, steamId, playerId);
            return true;
        }

        public void Update()
        {
            Command command;
            while (_messages.TryDequeue(out command))
            {
                Log.Info($"command: {command}");
                _listener.ProcessCommandOnServer(command);
            }
        }
    }
}