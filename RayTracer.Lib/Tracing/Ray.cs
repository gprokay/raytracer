using System;
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

        public bool IsSurfaceHit(Vector3 normal)
        {
            var dp = Vector3.Dot(Direction, normal) * -1;
            var isSurface = dp >= -0.1f;
            return isSurface;
        }

        public bool TryIntersectPlane(Plane plane, out Vector3 point)
        {
            point = default;
            var p0 = plane.Normal * plane.D;
            var dot = Vector3.Dot(Direction, plane.Normal);

            if (dot > -0.001 && dot < 0.001f) return false;

            var d = Vector3.Dot(p0 - StartPoint, plane.Normal) / Vector3.Dot(Direction, plane.Normal);

            if (d < 0 || d > Length && Length != float.PositiveInfinity) return false;

            point = d * Direction + StartPoint;

            return true;
        }

        public bool TryIntersectPlane(Vector3 normal, Vector3 p0, out Vector3 point)
        {
            point = default;
            var dot = Vector3.Dot(Direction, normal);

            if (dot > -0.001 && dot < 0.001f) return false;

            var d = Vector3.Dot(p0 - StartPoint, normal) / Vector3.Dot(Direction, normal);

            if (d < 0 || d > Length && Length != float.PositiveInfinity) return false;

            point = d * Direction + StartPoint;

            return true;
        }

        public bool TryIntersectTriangle(Vector3 v0, Vector3 v1, Vector3 v2, out float v, out float u, out float t)
        {
            v = default;
            u = default;
            t = default;
            var v0v1 = v1 - v0;
            var v0v2 = v2 - v0;
            var pvec = Vector3.Cross(Direction, v0v2);
            var det = Vector3.Dot(v0v1, pvec);
            if (MathF.Abs(det) < 0) return false;
            var invDet = 1 / det;
            var tvec = StartPoint - v0;
            u = Vector3.Dot(tvec, pvec) * invDet;
            if (u < 0 || u > 1) return false;

            var qvec = Vector3.Cross(tvec, v0v1);
            v = Vector3.Dot(Direction, qvec) * invDet;
            if (v < 0 || u + v > 1) return false;

            t = Vector3.Dot(v0v2, qvec) * invDet;
            return true;
        }
    }
}
