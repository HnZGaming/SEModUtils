using System;
using ProtoBuf;
using Sandbox.Game;
using VRageMath;

namespace HNZ.Utils.Communications
{
    [ProtoContract]
    public sealed class Command
    {
        [ProtoMember(1)]
        public string Header;

        [ProtoMember(2)]
        public string[] Arguments;

        [ProtoMember(3)]
        public ulong SteamId;

        [ProtoMember(4)]
        public long PlayerId;

        public Command()
        {
            Header = "";
            Arguments = Array.Empty<string>();
        }

        public Command(string header, string[] arguments, ulong steamId, long playerId)
        {
            PlayerId = playerId;
            Header = header;
            Arguments = arguments;
            SteamId = steamId;
        }

        public override string ToString()
        {
            return $"{{{nameof(Header)}: {Header}, {nameof(Arguments)}: {Arguments.SeqToString()}}}";
        }

        public void Respond(string author, Color color, string message)
        {
            MyVisualScriptLogicProvider.SendChatMessageColored(message, color, author, PlayerId);
        }
    }
}