using System;
using System.Linq;
using System.Numerics;

namespace RayTracer.Lib
{
    public class MeshObject : IRayTraceObject
    {
        public Mesh Mesh { get; }
        public IMaterial Material { get; }
        public Vector3 Center { get; private set; } = new Vector3(0, 0, 0);

        public MeshObject(Mesh mesh, IMaterial material)
        {
            Mesh = mesh;
            Material = material;
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

        public bool TryGetIntersection(Ray ray, out Intersection intersection)
        {
            intersection = default;
            intersection.Distance = float.PositiveInfinity;
            var intersects = false;

            var currentTriangles = Enumerable.Range(0, Mesh.Triangles.Length).ToArray();
            var boundedTriangles = (int[])null;

            if (Mesh.Bound != null && (boundedTriangles = Mesh.Bound.GetBoundedTriangles(ray)) == null)
            {
                return false;
            }

            if (boundedTriangles != null)
            {
                currentTriangles = boundedTriangles;
            }

            for (var i = 0; i < currentTriangles.Length; ++i)
            {
                var triangleIndex = currentTriangles[i];
                var triangle = Mesh.Triangles[triangleIndex];
                var v1 = Mesh.Vertices[triangle.V1];
                var v2 = Mesh.Vertices[triangle.V2];
                var v3 = Mesh.Vertices[triangle.V3];
                var n = Mesh.Normals[triangleIndex];

                if (TryIntersect(ray, v1, v2, v3, out var v, out var u, out var t))
                {
                    intersects = true;
                    var d = Vector3.Dot(v1 - ray.StartPoint, n) / Vector3.Dot(n, ray.Direction);
                    if (d < intersection.Distance)
                    {
                        intersects = true;
                        intersection.NormalVector = n;
                        intersection.Distance = d;
                        intersection.IntersectionPoint = d * ray.Direction + ray.StartPoint;
                    }
                }
            }

            return intersects && intersection.Distance > 0.001f && (intersection.Distance < ray.Length - 0.001f || ray.Length == float.PositiveInfinity);
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
            if (MathF.Abs(det) < 0) return false;
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
