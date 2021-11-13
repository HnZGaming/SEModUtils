using ProtoBuf;

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

        public Command()
        {
            Header = "";
            Arguments = new string[0];
        }

        public Command(string header, string[] arguments, ulong steamId)
        {
            Header = header;
            Arguments = arguments;
            SteamId = steamId;
        }

        public override string ToString()
        {
            return $"{{{nameof(Header)}: {Header}, {nameof(Arguments)}: {Arguments.SeqToString()}}}";
        }
    }
}