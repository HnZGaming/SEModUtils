using System.Collections.Generic;
using Sandbox.ModAPI;
using VRage.Game.ModAPI;

namespace HNZ.Utils
{
    public sealed class DamageSuppressor
    {
        readonly HashSet<IMyCubeGrid> _grids;
        bool _updatedOnce;

        public DamageSuppressor()
        {
            _grids = new HashSet<IMyCubeGrid>();
        }

        public void Close()
        {
            _grids.Clear();
        }

        public void Update()
        {
            if (!_updatedOnce)
            {
                _updatedOnce = true;
                OnFirstFrame();
            }
        }

        void OnFirstFrame()
        {
            MyAPIGateway.Session.DamageSystem.RegisterBeforeDamageHandler(
                int.MaxValue, (object t, ref MyDamageInformation i) =>
                    BeforeDamageApplied(t, ref i));
        }

        void BeforeDamageApplied(object target, ref MyDamageInformation info)
        {
            var block = target as IMyCubeBlock ?? (target as IMySlimBlock)?.FatBlock;
            if (block == null) return;

            if (_grids.Contains(block.CubeGrid))
            {
                info.Amount = 0;
            }
        }

        public void Add(IMyCubeGrid grid)
        {
            _grids.Add(grid);
        }

        public void Remove(IMyCubeGrid grid)
        {
            _grids.Remove(grid);
        }
    }
}