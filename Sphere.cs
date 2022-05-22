using System.Xml.Serialization;
using VRageMath;

namespace HNZ.Utils
{
    // better serializable version of sphere
    public sealed class Sphere
    {
        [XmlAttribute]
        public float X;

        [XmlAttribute]
        public float Y;

        [XmlAttribute]
        public float Z;

        [XmlAttribute]
        public float Radius;

        public Vector3 Center => new Vector3(X, Y, Z);

        public static implicit operator BoundingSphere(Sphere sphere) => new BoundingSphere
        {
            Center = sphere.Center,
            Radius = sphere.Radius,
        };

        public static implicit operator BoundingSphereD(Sphere sphere) => new BoundingSphereD
        {
            Center = sphere.Center,
            Radius = sphere.Radius,
        };
    }
}