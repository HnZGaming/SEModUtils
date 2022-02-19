using System.Collections.Generic;
using Sandbox.ModAPI;
using VRage.Game.ModAPI;

namespace HNZ.Utils
{
    public sealed class CharacterToPlayerLookup
    {
        readonly List<IMyPlayer> _tmpPlayers;
        readonly Dictionary<long, IMyPlayer> _characterIdToPlayers;

        public CharacterToPlayerLookup()
        {
            _tmpPlayers = new List<IMyPlayer>();
            _characterIdToPlayers = new Dictionary<long, IMyPlayer>();
        }

        public void Clear()
        {
            _tmpPlayers.Clear();
            _characterIdToPlayers.Clear();
        }

        public void Update()
        {
            MyAPIGateway.Players.GetPlayers(_tmpPlayers);
            _characterIdToPlayers.Clear();
            foreach (var player in _tmpPlayers)
            {
                if (player.Character != null)
                {
                    _characterIdToPlayers[player.Character.EntityId] = player;
                }
            }

            _tmpPlayers.Clear();
        }

        public bool TryGetPlayer(IMyCharacter character, out IMyPlayer player)
        {
            return _characterIdToPlayers.TryGetValue(character.EntityId, out player);
        }

        public bool TryGetFaction(IMyCharacter character, out IMyFaction faction)
        {
            faction = default(IMyFaction);
            IMyPlayer player;
            if (!_characterIdToPlayers.TryGetValue(character.EntityId, out player)) return false;

            faction = MyAPIGateway.Session.Factions.TryGetPlayerFaction(player.IdentityId);
            return faction != null;
        }
    }
}