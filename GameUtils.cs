using System;
using System.Collections.Generic;
using HNZ.Utils.Logging;
using Sandbox.Game.Entities;
using Sandbox.ModAPI;
using VRage.Game.Components;
using VRage.Game.ModAPI;
using VRage.ModAPI;

namespace HNZ.Utils
{
    public static class GameUtils
    {
        static readonly Logger Log = LoggerManager.Create(nameof(GameUtils));

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

        public static void GetCharacters(IMyEntity entity, ICollection<IMyCharacter> characters)
        {
            var grid = entity as IMyCubeGrid;
            if (grid != null)
            {
                var cockpits = ((MyCubeGrid)grid).OccupiedBlocks;
                foreach (var cockpit in cockpits)
                {
                    characters.Add(cockpit.Pilot);
                }
            }

            var character = entity as IMyCharacter;
            if (character != null)
            {
                characters.Add(character);
            }
        }

        public static bool IsAdmin(ulong steamId)
        {
            return MyAPIGateway.Session.IsServer ||
                   MyAPIGateway.Session.IsUserAdmin(steamId);
        }

        public static void SetPower(this IMyCubeGrid self, bool on)
        {
            var grid = (MyCubeGrid)self;
            if ((!grid.IsPowered && on) || (grid.IsPowered && !on))
            {
                grid.SwitchPower();
            }
        }

        public static int GetIntValue(this MyModStorageComponentBase self, Guid guid)
        {
            string forgeCountStr;
            self.TryGetValue(guid, out forgeCountStr);

            int forgeCount;
            int.TryParse(forgeCountStr ?? "", out forgeCount);
            return forgeCount;
        }
    }
}