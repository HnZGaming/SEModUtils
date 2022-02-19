using System.Collections.Generic;
using Sandbox.Game.Entities.Character;
using Sandbox.Game.World;
using Sandbox.ModAPI;
using VRage.Game.ModAPI;

namespace HNZ.Utils
{
    public static class GameUtils
    {
        public static bool TryGetPlayerBySteamId(ulong steamId, out IMyPlayer player)
        {
            var players = new List<IMyPlayer>();
            MyAPIGateway.Players.GetPlayers(players);
            foreach (var p in players)
            {
                if (p.SteamUserId == steamId)
                {
                    player = p;
                    return true;
                }
            }

            player = default(IMyPlayer);
            return false;
        }
    }
}