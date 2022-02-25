using System.Collections.Generic;
using Sandbox.ModAPI;
using VRage.Game.ModAPI;
using VRage.ModAPI;

namespace HNZ.Utils
{
    public sealed class JumpDriveSuppressor
    {
        readonly List<IMyJumpDrive> _inboundSafeZoneBlocks;

        public JumpDriveSuppressor()
        {
            _inboundSafeZoneBlocks = new List<IMyJumpDrive>();
        }

        public void Clear()
        {
            _inboundSafeZoneBlocks.Clear();
        }

        public void Collect(IEnumerable<IMyEntity> inboundEntities)
        {
            _inboundSafeZoneBlocks.Clear();

            foreach (var inboundEntity in inboundEntities)
            {
                var safeZoneBlock = inboundEntity as IMyJumpDrive;
                if (safeZoneBlock != null)
                {
                    _inboundSafeZoneBlocks.Add(safeZoneBlock);
                }

                var grid = inboundEntity as IMyCubeGrid;
                if (grid != null)
                {
                    foreach (var inlineSafeZoneBlock in grid.GetFatBlocks<IMyJumpDrive>())
                    {
                        _inboundSafeZoneBlocks.Add(inlineSafeZoneBlock);
                    }
                }
            }
        }

        public void Suppress()
        {
            foreach (var safeZoneBlock in _inboundSafeZoneBlocks)
            {
                safeZoneBlock.Enabled = false;
            }
        }
    }
}