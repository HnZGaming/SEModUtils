using System;
using System.Collections.Generic;
using HNZ.Utils.Logging;
using HNZ.Utils.Pools;
using Sandbox.Game.Entities;
using Sandbox.Game.EntityComponents;
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

        public static bool HasAnyGridsInSphere(BoundingSphereD sphere)
        {
            var entities = ListPool<MyEntity>.Get();
            try
            {
                MyGamePruningStructure.GetAllTopMostEntitiesInSphere(ref sphere, entities);
                foreach (var entity in entities)
                {
                    if (entity is IMyCubeGrid)
                    {
                        return true;
                    }
                }

                return false;
            }
            finally
            {
                ListPool<MyEntity>.Release(entities);
            }
        }

        public static bool HasCharactersInRadius(this IMyEntity self, float radius)
        {
            var entities = ListPool<MyEntity>.Get();
            var characters = ListPool<IMyCharacter>.Get();

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

        public static bool TryGetRandomPosition(BoundingSphereD search, float clearance, float maxGravity, out Vector3D position)
        {
            // get a random position
            position = MathUtils.GetRandomPosition(search);

            // check for gravity
            float gravityInterference;
            var gravity = MyAPIGateway.Physics.CalculateNaturalGravityAt(position, out gravityInterference);
            if (gravity.Length() > maxGravity) return false;

            // check for space
            var sphere = new BoundingSphereD(position, clearance);
            if (HasAnyGridsInSphere(sphere)) return false;

            return true;
        }

        public static void PlaySound(string cueName)
        {
            var character = MyAPIGateway.Session?.LocalHumanPlayer?.Character;
            if (character == null) return;

            var emitter = new MyEntity3DSoundEmitter(character as MyEntity);
            var sound = new MySoundPair(cueName);
            emitter.PlaySound(sound);
        }

        public static bool TestSurfaceFlat(MyPlanet planet, Vector3D origin, float width, float error)
        {
            var baseLength = (planet.PositionComp.GetPosition() - origin).Length();
            for (var x = 0; x < 2; x++)
            for (var y = 0; y < 2; y++)
            for (var z = 0; z < 2; z++)
            {
                var offset = new Vector3D
                {
                    X = (x * 2 - 1) * width,
                    Y = (y * 2 - 1) * width,
                    Z = (z * 2 - 1) * width,
                };

                var point = origin + offset;
                var surfacePoint = planet.GetClosestSurfacePointGlobal(point);
                var length = (planet.PositionComp.GetPosition() - surfacePoint).Length();
                if (Math.Abs(length - baseLength) > error) return false;
            }

            return true;
        }

        public static void UpdateStorageValue(this IMyEntity self, Guid key, string value)
        {
            if (self.Storage == null)
            {
                self.Storage = new MyModStorageComponent();
            }

            self.Storage[key] = value;
        }

        public static bool TryGetStorageValue(this IMyEntity self, Guid key, out string value)
        {
            if (self.Storage == null)
            {
                value = null;
                return false;
            }

            return self.Storage.TryGetValue(key, out value);
        }
    }
}