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

        public bool TryIntersect(Plane plane, out Vector3 point)
        {
            point = default;
            var p0 = plane.Normal * plane.D;
            var dot = Vector3.Dot(Direction, plane.Normal);
            if (dot > -0.001f && dot < 0.001f) return false;
            var d = Vector3.Dot(p0 - StartPoint, plane.Normal) / Vector3.Dot(Direction, plane.Normal);
            if (d < 0.001f || d > Length - 0.001f && Length != float.PositiveInfinity) return false;

            point = d * Direction + StartPoint;
            return true;
        }
    }
}
