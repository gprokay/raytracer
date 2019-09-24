using System;
using System.Linq;
using System.Numerics;

namespace RayTracer.Lib
{
    public class MeshObject : IRayTraceObject
    {
        private readonly Vector3[] vectors;
        private (int V1, int V2, int V3)[] triangles;

        public IMaterial Material { get; }


        public MeshObject(Vector3[] vectors, (int V1, int V2, int V3)[] triangles, IMaterial material)
        {
            this.vectors = vectors;
            this.triangles = triangles;
            Material = material;
        }

        public void ReorderTriangles(Vector3 point)
        {
            triangles = triangles
                .Select(t => new { D = (MathF.Abs((point - vectors[t.V1]).Length()) + MathF.Abs((point - vectors[t.V2]).Length()) + MathF.Abs((point - vectors[t.V3]).Length())) / 3, Triangle = t })
                .OrderBy(t => t.D)
                .Select(t => t.Triangle)
                .ToArray();
        }

        public void Rotate(float x, float y, float z)
        {
            var rotX = Matrix4x4.CreateRotationX(x);
            var rotY = Matrix4x4.CreateRotationY(y);
            var rotZ = Matrix4x4.CreateRotationZ(z);

            for (int i = 0; i < vectors.Length; ++i)
            {
                var vr = Vector3.Transform(vectors[i], rotX);
                vr = Vector3.Transform(vr, rotY);
                vr = Vector3.Transform(vr, rotZ);
                vectors[i] = vr;
            }
        }

        public static Vector3 RotateVector(Vector3 v, float x, float y, float z)
        {
            var rotX = Matrix4x4.CreateRotationX(x);
            var rotY = Matrix4x4.CreateRotationY(y);
            var rotZ = Matrix4x4.CreateRotationZ(z);

            var vr = Vector3.Transform(v, rotX);
            vr = Vector3.Transform(vr, rotY);
            return Vector3.Transform(vr, rotZ);
        }

        public void Scale(float x, float y, float z)
        {
            for (int i = 0; i < vectors.Length; ++i)
            {
                vectors[i].X = vectors[i].X * x;
                vectors[i].Y = vectors[i].Y * y;
                vectors[i].Z = vectors[i].Z * z;
            }
        }

        public void Normalize()
        {
            var maxX = vectors.Select(v => v.X).Max();
            var maxY = vectors.Select(v => v.Y).Max();
            var maxZ = vectors.Select(v => v.Z).Max();

            for (int i = 0; i < vectors.Length; ++i)
            {
                vectors[i].X = vectors[i].X / maxX;
                vectors[i].Y = vectors[i].Y / maxY;
                vectors[i].Z = vectors[i].Z / maxZ;
            }
        }

        public bool TryGetIntersection(Ray ray, out Intersection intersection)
        {
            intersection = default;
            intersection.Distance = float.PositiveInfinity;
            var hasI = false;
            foreach (var triangle in triangles)
            {
                var v1 = vectors[triangle.V1];
                var v2 = vectors[triangle.V2];
                var v3 = vectors[triangle.V3];
                if (TryIntersect(ray, v1, v2, v3, out var v, out var u, out var t))
                {
                    hasI = true;
                    var n = Vector3.Normalize(Vector3.Cross(v2 - v1, v3 - v1));
                    var d = Vector3.Dot((v1 - ray.StartPoint), n) / Vector3.Dot(n, ray.Direction);
                    if (d < intersection.Distance)
                    {
                        hasI = true;
                        intersection.NormalVector = n;
                        intersection.Distance = d;
                        intersection.IntersectionPoint = d * ray.Direction + ray.StartPoint;
                        //return intersection.Distance > 0.001f && (intersection.Distance < ray.Length - 0.001f || ray.Length == float.PositiveInfinity);
                    }
                }
            }

            return hasI && intersection.Distance > 0.001f && (intersection.Distance < ray.Length - 0.001f || ray.Length == float.PositiveInfinity);
        }

        private static bool TryIntersect(Ray ray, Vector3 v0, Vector3 v1, Vector3 v2, out float v, out float u, out float t)
        {
            v = default;
            u = default;
            t = default;
            var v0v1 = v1 - v0;
            var v0v2 = v2 - v0;
            var pvec = Vector3.Cross(ray.Direction, v0v2);
            var det = Vector3.Dot(v0v1, pvec);
            if (MathF.Abs(det) < 0.001f) return false;
            var invDet = 1 / det;
            var tvec = ray.StartPoint - v0;
            u = Vector3.Dot(tvec, pvec) * invDet;
            if (u < 0 || u > 1) return false;

            var qvec = Vector3.Cross(tvec, v0v1);
            v = Vector3.Dot(ray.Direction, qvec) * invDet;
            if (v < 0 || u + v > 1) return false;

            t = Vector3.Dot(v0v2, qvec) * invDet;
            return true;
        }
    }
}
