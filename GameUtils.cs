using System;
using System.Collections.Generic;
using HNZ.Utils.Logging;
using HNZ.Utils.Pools;
using Sandbox.Game;
using Sandbox.Game.Entities;
using Sandbox.Game.EntityComponents;
using Sandbox.ModAPI;
using VRage;
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

        public static void GetCharacters(this IMyEntity self, float radius, ICollection<IMyCharacter> characters)
        {
            var entities = ListPool<MyEntity>.Get();

            var sphere = new BoundingSphereD(self.GetPosition(), radius);
            MyGamePruningStructure.GetAllEntitiesInSphere(ref sphere, entities);
            foreach (var entity in entities)
            {
                GetCharacters(entity, characters);
            }

            ListPool<MyEntity>.Release(entities);
        }

        public static bool HasCharactersInRadius(this IMyEntity self, float radius)
        {
            var characters = ListPool<IMyCharacter>.Get();
            self.GetCharacters(radius, characters);

            foreach (var character in characters)
            {
                if (character.IsPlayer)
                {
                    return true;
                }
            }

            return false;
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

        public static bool EverySeconds(float seconds)
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

        public static void DumpAllInventories(IMyEntity entity)
        {
            for (var i = 0; i < entity.InventoryCount; i++)
            {
                var inventory = (MyInventory)entity.GetInventory(i);
                DumpInventory(inventory);
            }
        }

        public static void DumpInventory(MyInventory inventory)
        {
            var entity = inventory.Entity;
            foreach (var item in inventory.GetItems())
            {
                var boundingBox = entity.PositionComp.WorldAABB;
                MyFloatingObjects.EnqueueInventoryItemSpawn(item, boundingBox, Vector3D.Zero);
            }

            inventory.Clear(true);
        }

        public static Dictionary<string, MyFixedPoint> GetAllInventoryItems(IMyEntity entity)
        {
            var d = new Dictionary<string, MyFixedPoint>();
            for (var i = 0; i < entity.InventoryCount; i++)
            {
                var inventory = (MyInventory)entity.GetInventory(i);
                foreach (var item in inventory.GetItems())
                {
                    var name = $"{item.Content.TypeId}.{item.Content.SubtypeName}";
                    MyFixedPoint amount;
                    d.TryGetValue(name, out amount);
                    amount += item.Amount;
                    d[name] = amount;
                }
            }

            return d;
        }

        public static bool IsOwnedByAnyFactions(this IMyCubeGrid grid, ICollection<long> factionIds)
        {
            foreach (var ownerId in grid.BigOwners)
            {
                var faction = MyAPIGateway.Session.Factions.TryGetPlayerFaction(ownerId);
                if (faction == null) continue;

                if (factionIds.Contains(faction.FactionId))
                {
                    return true;
                }
            }

            return false;
        }

        public static bool GetGroupOwnerIds(this IMyCubeGrid self, ISet<long> ownerIds, GridLinkTypeEnum linkType)
        {
            var added = false;
            var grids = ListPool<IMyCubeGrid>.Get();
            var group = self.GetGridGroup(linkType);
            group.GetGrids(grids);
            foreach (var grid in grids)
            foreach (var ownerId in grid.BigOwners)
            {
                added |= ownerIds.Add(ownerId);
            }

            ListPool<IMyCubeGrid>.Release(grids);
            return added;
        }

        public static bool IsDamageAllowed(MyEntity entity)
        {
            return MySessionComponentSafeZones.IsActionAllowed(
                entity,
                CastProhibit(MySessionComponentSafeZones.AllowedActions, 1));
        }

        static T CastProhibit<T>(T ptr, object val) => (T)val;

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

        public static void GetBigOwnerFactions(this IMyCubeGrid self, ICollection<IMyFaction> factions)
        {
            foreach (var bigOwner in self.BigOwners)
            {
                var faction = MyAPIGateway.Session.Factions.TryGetPlayerFaction(bigOwner);
                if (faction != null)
                {
                    factions.Add(faction);
                }
            }
        }

        public static bool TryGetFaction(this IMyCubeGrid self, out IMyFaction faction)
        {
            var factions = ListPool<IMyFaction>.Get();
            try
            {
                self.GetBigOwnerFactions(factions);
                if (factions.Count == 0)
                {
                    faction = default(IMyFaction);
                    return false;
                }

                faction = factions[0];
                return true;
            }
            finally
            {
                ListPool<IMyFaction>.Release(factions);
            }
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

        public static bool TestStorageKeyValue(this IMyEntity self, Guid key, string value)
        {
            string v;
            return self.TryGetStorageValue(key, out v) && v == value;
        }

        public static bool ReadFile(string filePath, out string fileContent)
        {
            if (!MyAPIGateway.Utilities.FileExistsInWorldStorage(filePath, typeof(string)))
            {
                fileContent = default(string);
                return false;
            }

            using (var reader = MyAPIGateway.Utilities.ReadFileInWorldStorage(filePath, typeof(string)))
            {
                fileContent = reader.ReadToEnd();
                return true;
            }
        }

        public static void WriteFile(string filePath, string fileContent)
        {
            using (var writer = MyAPIGateway.Utilities.WriteFileInWorldStorage(filePath, typeof(string)))
            {
                writer.Write(fileContent);
            }
        }

        // if this is the server in multi-player -> false
        // if this is a client in multi-player -> true
        // if this is a client in single-player -> true
        public static bool IsClient =>
            !MyAPIGateway.Utilities.IsDedicated ||
            !MyAPIGateway.Session.IsServer;
    }
}