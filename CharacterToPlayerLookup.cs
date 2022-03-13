using System.Collections.Generic;
using HNZ.Utils.Pools;
using Sandbox.ModAPI;
using VRage.Game.Entity;
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
    }
}