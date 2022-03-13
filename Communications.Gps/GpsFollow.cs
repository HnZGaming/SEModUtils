using VRage.Game.ModAPI;
using VRageMath;

namespace HNZ.Utils.Communications.Gps
{
    public sealed class GpsFollow
    {
        readonly IMyGps _gps;
        Vector3D _velocity;
        Vector3D _targetPosition;

        public GpsFollow(IMyGps gps)
        {
            _gps = gps;
        }

        public void SetTargetPosition(Vector3D position)
        {
            _targetPosition = position;
        }

        public void Update()
        {
            _gps.Coords = MathUtils.SmoothDamp(_gps.Coords, _targetPosition, ref _velocity, 1f, float.MaxValue, 0.016f);
        }
    }
}