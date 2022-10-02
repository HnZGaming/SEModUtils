using System;
using System.Collections.Generic;
using HNZ.Utils.Logging;
using VRage.Game.ModAPI;
using VRageMath;

namespace HNZ.Utils.MES
{
    public sealed class MESGrid
    {
        static readonly Logger Log = LoggerManager.Create(nameof(MESGrid));
        static readonly Guid IdKey = Guid.Parse("6BFEA3E4-7B06-460C-ADD1-C1A66EB7B5E9");
        const float TimeoutSecs = 10;

        readonly MESApi _mesApi;
        readonly string _id;
        readonly string _spawnGroup;
        IMyCubeGrid _grid;
        bool _cleanupIgnored;
        DateTime? _spawnedTime;
        bool _spawning;
        MatrixD _spawningMatrix;

        public MESGrid(MESApi mesApi, string id, string spawnGroup)
        {
            _mesApi = mesApi;
            _id = id;
            _spawnGroup = spawnGroup;
        }

        public static bool TryCreateFromExistingGrid(MESApi mesApi, string id, string spawnGroup, IMyCubeGrid existingGrid, out MESGrid grid)
        {
            if (!TestIdentity(existingGrid, spawnGroup, id))
            {
                grid = null;
                return false;
            }

            grid = new MESGrid(mesApi, id, spawnGroup);
            grid.SetGrid(existingGrid);
            return true;
        }

        public bool Closed => _grid == null;

        public bool TryInitialize(MatrixD spawnMatrix, bool ignoreSafetyCheck)
        {
            Log.Info($"spawning: {_spawnGroup} at {spawnMatrix.Translation}");

            if (!_mesApi.CustomSpawnRequest(
                    new List<string> { _spawnGroup },
                    spawnMatrix,
                    Vector3.Zero,
                    ignoreSafetyCheck,
                    null,
                    nameof(MesCustomBossSpawner)))
            {
                return false;
            }

            _mesApi.RegisterSuccessfulSpawnAction(OnMesAnySuccessfulSpawn, true);
            _spawnedTime = DateTime.UtcNow;
            _spawning = true;
            _spawningMatrix = spawnMatrix;
            return true;
        }

        public void Close()
        {
            var wasClosed = Closed;

            _grid.OrNull()?.Close();
            _grid = null;

            _spawning = false;
            _spawnedTime = null;

            if (!wasClosed)
            {
                _mesApi.RegisterSuccessfulSpawnAction(OnMesAnySuccessfulSpawn, false);
                Log.Info($"closed grid: {_spawnGroup}");
            }
        }

        public void Update()
        {
            var timeout = _spawnedTime + TimeSpan.FromSeconds(TimeoutSecs) - DateTime.UtcNow;
            if (_spawning && _spawnedTime != null && _grid == null && timeout.HasValue && timeout.Value.TotalSeconds < 0)
            {
                Log.Warn($"timeout: {_spawnGroup}");
                Close();
                return;
            }

            if (Closed) return;

            // deleted or whatever
            if (_grid != null && _grid.Closed)
            {
                Log.Info($"boss grid closed by someone else: {_grid.DisplayName}");
                Close();
                return;
            }

            if (!_cleanupIgnored)
            {
                _cleanupIgnored = _mesApi.SetSpawnerIgnoreForDespawn(_grid, true);
                if (_cleanupIgnored)
                {
                    Log.Info($"cleanup ignored: {_spawnGroup}");
                }
            }
        }

        void OnMesAnySuccessfulSpawn(IMyCubeGrid grid)
        {
            // not mine: wrong spawn group
            if (!TestIdentity(grid, _spawnGroup)) return;

            Log.Info($"spawn found: {grid.DisplayName} for spawn group: {_spawnGroup}");

            // not mine: wrong position
            var gridPos = grid.WorldMatrix.Translation;
            if (Vector3D.Distance(gridPos, _spawningMatrix.Translation) > 500)
            {
                Log.Warn($"different position: {_spawnGroup}, {_id}");
                return;
            }

            grid.UpdateStorageValue(IdKey, _id);
            SetGrid(grid);
        }

        void SetGrid(IMyCubeGrid grid)
        {
            _grid = grid;
            _cleanupIgnored = false;
            _spawning = false;
            _spawnedTime = null;

            _mesApi.RegisterSuccessfulSpawnAction(OnMesAnySuccessfulSpawn, false);
            _mesApi.RegisterDespawnWatcher(_grid, OnBossDispawned);
        }

        void OnBossDispawned(IMyCubeGrid grid, string type)
        {
            Log.Info($"dispawned: {grid.DisplayName}, cause: {type}");
            Close();
        }

        public void CleanUpIfFarFromCharacters(float radius = 0f)
        {
            if (Closed) return;

            if (radius == 0 || (_grid.OrNull()?.HasCharactersInRadius(radius) ?? false))
            {
                Close();
            }
        }

        static bool TestIdentity(IMyCubeGrid grid, string spawnGroup, string id = null)
        {
            if (grid == null) return false;
            if (!NpcData.TestSpawnGroup(grid, spawnGroup)) return false;

            if (id != null)
            {
                string existingId;
                if (!grid.TryGetStorageValue(IdKey, out existingId)) return false;
                if (existingId != id) return false;
            }

            return true;
        }
    }
}