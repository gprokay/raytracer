using System.Diagnostics;
using System.Numerics;

namespace RayTracer.Lib
{
    [DebuggerDisplay("C= {StartPoint}; D= {Direction}; L= {Length}")]
    public struct Ray
    {
        public Vector3 Direction;

        public Vector3 StartPoint;

        public float Length;

        public Ray(Vector3 startPoint, Vector3 direction, float length = float.PositiveInfinity)
        {
            StartPoint = startPoint;
            Length = length;
            Direction = Vector3.Normalize(direction);
        }
    }
}
