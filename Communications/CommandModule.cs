using System;
using System.Collections.Concurrent;
using System.Text.RegularExpressions;
using Sandbox.ModAPI;

namespace HNZ.Utils.Communications
{
    public sealed class CommandModule
    {
        static readonly Logger Log = new Logger(typeof(CommandModule));

        readonly ProtobufModule _protobuf;
        readonly ICommandListener _listener;
        readonly Regex _commandFormat;
        readonly ConcurrentQueue<Command> _messages;

        public CommandModule(ProtobufModule protobuf, string symbol, ICommandListener listener)
        {
            _protobuf = protobuf;
            _listener = listener;
            _commandFormat = new Regex($@"^(?:/|!){symbol} (.+?)$");
            _messages = new ConcurrentQueue<Command>();
        }

        public void Initialize()
        {
            MyAPIGateway.Utilities.MessageEntered += OnChatMessageEntered;
            _protobuf.OnChatCommandSentFromClient += OnChatCommandSentFromClient;
        }

        public void Close()
        {
            MyAPIGateway.Utilities.MessageEntered -= OnChatMessageEntered;
            _protobuf.OnChatCommandSentFromClient -= OnChatCommandSentFromClient;
        }

        void OnChatMessageEntered(string messageText, ref bool sendToOthers)
        {
            Command command;
            if (!TryParseCommand(messageText, out command)) return;

            if (_listener.TryProcessClientCommand(command)) return;

            if (MyAPIGateway.Session.IsServer)
            {
                _messages.Enqueue(command);
            }
            else
            {
                _protobuf.SendChatCommandToServer(command);
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

            command = new Command(head, args, MyAPIGateway.Session.LocalHumanPlayer.SteamUserId);
            return true;
        }

        void OnChatCommandSentFromClient(Command command)
        {
            _messages.Enqueue(command);
        }

        public void FrameUpdate()
        {
            Command command;
            while (_messages.TryDequeue(out command))
            {
                Log.Info($"command: {command}");
                _listener.ProcessServerCommand(command);
            }
        }
    }
}