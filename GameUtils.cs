using System;
using System.Collections.Generic;
using HNZ.Utils.Logging;
using HNZ.Utils.Pools;
using Sandbox.Game.Entities;
using Sandbox.ModAPI;
using VRage.Game.Components;
using VRage.Game.Entity;
using VRage.Game.ModAPI;
using VRage.Library.Utils;
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

        public static bool HasCharactersARound(this IMyEntity self, float radius)
        {
            var entities = ListPool<MyEntity>.Create();
            var characters = ListPool<IMyCharacter>.Create();

            var sphere = new BoundingSphereD(self.GetPosition(), radius);
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

            return count > 0;
        }

        public static bool IsNullOrClosed(this IMyEntity entity)
        {
            if (entity == null) return true;
            if (entity.Closed) return true;
            return false;
        }

        public static T OrNull<T>(this T entity) where T : class, IMyEntity
        {
            if (entity == null) return null;
            if (entity.Closed) return null;
            return entity;
        }

        public static bool EverySeconds(int seconds)
        {
            return MyAPIGateway.Session.GameplayFrameCounter % (seconds * 60) == 0;
        }

        public static bool TryGetRandomPosition(Vector3D origin, float searchRadius, float clearanceRadius, out Vector3D position)
        {
            for (var i = 0; i < 100; i++)
            {
                // get a random position
                var rand = (float)MyRandom.Instance.Next(0, 100) / 100;
                var radius = searchRadius * rand;
                position = origin + MathUtils.GetRandomUnitDirection() * radius;

                // check for gravity
                float gravityInterference;
                var gravity = MyAPIGateway.Physics.CalculateNaturalGravityAt(position, out gravityInterference);
                if (gravity != Vector3.Zero) continue;

                // check for space
                var sphere = new BoundingSphereD(position, clearanceRadius);
                if (GetEntityCountInSphere(sphere) > 0) continue;

                return true;
            }

            position = default(Vector3D);
            return false;
        }

        public static void PlaySound(string cueName)
        {
            var character = MyAPIGateway.Session?.LocalHumanPlayer?.Character;
            if (character == null) return;

            var emitter = new MyEntity3DSoundEmitter(character as MyEntity);
            var sound = new MySoundPair(cueName);
            emitter.PlaySound(sound);
        }
    }
}