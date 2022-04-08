using System.Collections.Generic;
using HNZ.Utils.Pools;
using Sandbox.Game.Entities;
using SpaceEngineers.Game.ModAPI;
using VRage.Game.Entity;
using VRage.Game.ModAPI;
using VRage.ModAPI;
using VRageMath;

namespace HNZ.Utils
{
    public sealed class SafeZoneSuppressor
    {
        readonly List<MySafeZone> _inboundSafeZones;
        readonly List<IMySafeZoneBlock> _inboundSafeZoneBlocks;

        public SafeZoneSuppressor()
        {
            _inboundSafeZones = new List<MySafeZone>();
            _inboundSafeZoneBlocks = new List<IMySafeZoneBlock>();
        }

        public void Clear()
        {
            _inboundSafeZones.Clear();
            _inboundSafeZoneBlocks.Clear();
        }

        public void Collect(IEnumerable<IMyEntity> inboundEntities)
        {
            _inboundSafeZones.Clear();
            _inboundSafeZoneBlocks.Clear();

            foreach (var inboundEntity in inboundEntities)
            {
                var safeZone = inboundEntity as MySafeZone;
                if (safeZone != null)
                {
                    _inboundSafeZones.Add(safeZone);
                }

                var safeZoneBlock = inboundEntity as IMySafeZoneBlock;
                if (safeZoneBlock != null)
                {
                    _inboundSafeZoneBlocks.Add(safeZoneBlock);
                }

                var grid = inboundEntity as IMyCubeGrid;
                if (grid != null)
                {
                    foreach (var inlineSafeZoneBlock in grid.GetFatBlocks<IMySafeZoneBlock>())
                    {
                        _inboundSafeZoneBlocks.Add(inlineSafeZoneBlock);
                    }
                }
            }
        }

        public void Suppress()
        {
            foreach (var safeZone in _inboundSafeZones)
            {
                safeZone.Enabled = false;
            }

            foreach (var safeZoneBlock in _inboundSafeZoneBlocks)
            {
                safeZoneBlock.EnableSafeZone(false);
            }
        }

        public void CollectInSphere(ref BoundingSphereD sphere)
        {
            var entities = ListPool<MyEntity>.Get();

            MyGamePruningStructure.GetAllEntitiesInSphere(ref sphere, entities);
            Collect(entities);

            ListPool<MyEntity>.Release(entities);
        }
    }
}