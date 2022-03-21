using System;
using System.Collections.Generic;
using HNZ.Utils.Logging;
using HNZ.Utils.Pools;
using Sandbox.Game.Entities;
using Sandbox.ModAPI;
using VRage.Game.Components;
using VRage.Game.Entity;
using VRage.Game.ModAPI;
using VRage.ModAPI;
using VRageMath;

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

        public static int GetEntityCountInSphere(BoundingSphereD sphere)
        {
            var entities = ListPool<MyEntity>.Create();
            MyGamePruningStructure.GetAllEntitiesInSphere(ref sphere, entities);
            var entityCount = entities.Count;
            ListPool<MyEntity>.Release(entities);
            return entityCount;
        }

        public static int GetPlayerCharacterCountInSphere(BoundingSphereD sphere)
        {
            var entities = ListPool<MyEntity>.Create();
            var characters = ListPool<IMyCharacter>.Create();

            MyGamePruningStructure.GetAllEntitiesInSphere(ref sphere, entities);
            foreach (var entity in entities)
            {
                GetCharacters(entity, characters);
            }

            var count = 0;
            foreach (var character in characters)
            {
                if (character.IsPlayer)
                {
                    count += 1;
                }
            }

            ListPool<MyEntity>.Release(entities);
            ListPool<IMyCharacter>.Release(characters);

            return count;
        }

        public static bool IsNullOrClosed(this IMyEntity entity)
        {
            if (entity == null) return true;
            if (entity.Closed) return true;
            return false;
        }

        public static bool EverySeconds(int seconds)
        {
            return MyAPIGateway.Session.GameplayFrameCounter % (seconds * 60) == 0;
        }
    }
}