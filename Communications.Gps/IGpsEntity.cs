using VRageMath;

namespace HNZ.Utils.Communications.Gps
{
    public interface IGpsEntity
    {
        long Id { get; }
        string Name { get; }
        string Description { get; }
        Vector3D Position { get; }
        double Radius { get; }
        Color Color { get; }
        long EntityId { get; }
    }
}