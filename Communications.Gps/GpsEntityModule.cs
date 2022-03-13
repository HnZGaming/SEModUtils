using System.Collections.Generic;

namespace HNZ.Utils.Communications.Gps
{
    public sealed class GpsEntityModule
    {
        readonly GpsModule _gpsModule;
        readonly HashSet<IGpsEntity> _gpsEntities;

        public GpsEntityModule(GpsModule gpsModule)
        {
            _gpsModule = gpsModule;
            _gpsEntities = new HashSet<IGpsEntity>();
        }

        public void Clear()
        {
            _gpsEntities.Clear();
        }

        public void Track(IGpsEntity entity)
        {
            _gpsEntities.Add(entity);
        }

        public void UntrackAndSendRemove(IGpsEntity entity)
        {
            if (_gpsEntities.Remove(entity))
            {
                _gpsModule.SendRemoveGps(entity.Id);
            }
        }

        public void SendAddOrUpdate()
        {
            foreach (var gpsEntity in _gpsEntities)
            {
                _gpsModule.SendAddOrUpdateGps(new GpsSource
                {
                    Id = gpsEntity.Id,
                    Name = gpsEntity.Name,
                    Description = gpsEntity.Description,
                    Position = gpsEntity.Position,
                    Radius = gpsEntity.Radius,
                    Color = gpsEntity.Color,
                });
            }
        }
    }
}