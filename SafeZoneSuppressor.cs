using System.Collections.Generic;
using HNZ.Utils.Logging;
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
        public interface IFilter
        {
            bool CanSuppress(MySafeZone safeZone);
            bool CanSuppress(IMySafeZoneBlock mySafeZoneBlock);
        }

        static readonly Logger Log = LoggerManager.Create(nameof(SafeZoneSuppressor));

        readonly IFilter _filter;
        readonly List<MySafeZone> _inboundSafeZones;
        readonly List<IMySafeZoneBlock> _inboundSafeZoneBlocks;

        public SafeZoneSuppressor(IFilter filter)
        {
            _filter = filter;
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
                    // delegate to block filter
                    if (safeZone.SafeZoneBlockId != 0) continue;

                    if (_filter.CanSuppress(safeZone))
                    {
                        _inboundSafeZones.Add(safeZone);
                    }
                }

                var grid = inboundEntity as IMyCubeGrid;
                if (grid != null)
                {
                    foreach (var safeZoneBlock in grid.GetFatBlocks<IMySafeZoneBlock>())
                    {
                        if (_filter.CanSuppress(safeZoneBlock))
                        {
                            _inboundSafeZoneBlocks.Add(safeZoneBlock);
                        }
                    }
                }
            }
        }

        public void Suppress()
        {
            foreach (var safeZoneBlock in _inboundSafeZoneBlocks)
            {
                if (safeZoneBlock.Enabled)
                {
                    safeZoneBlock.Enabled = false;
                    Log.Info($"suppressed safe zone block: {safeZoneBlock.CubeGrid?.DisplayName}");
                }
            }

            foreach (var safeZone in _inboundSafeZones)
            {
                if (safeZone.Enabled)
                {
                    safeZone.Enabled = false;
                    Log.Info($"suppressed safe zone: {safeZone.DisplayName ?? safeZone.Name}");
                }
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