using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;

namespace RayTracer.Lib
{
    public class MeshObject : IRayTraceObject
    {
        private class MeshBound
        {
            private static Vector3 XNormal = new Vector3(1, 0, 0);
            private static Vector3 YNormal = new Vector3(0, 1, 0);
            private static Vector3 ZNormal = new Vector3(0, 0, 1);

            [DebuggerDisplay("C= {Center}")]
            private class InnerBound
            {
                public int Xp;
                public int Xn;
                public int Yp;
                public int Yn;
                public int Zp;
                public int Zn;
                public int[] Triangles;
                public Vector3 Center;

                public bool IsIn(Vector3 v, List<Plane> xPlanes, List<Plane> yPlanes, List<Plane> zPlanes, float bias)
                {
                    return v.X >= xPlanes[Xn].D - bias && v.X <= xPlanes[Xp].D + bias
                        && v.Y >= yPlanes[Yn].D - bias && v.Y <= yPlanes[Yp].D + bias
                        && v.Z >= zPlanes[Zn].D - bias && v.Z <= zPlanes[Zp].D + bias;
                }

                public bool Intersects(Ray ray, List<Plane> xPlanes, List<Plane> yPlanes, List<Plane> zPlanes)
                {
                    var negXPlane = xPlanes[Xn];
                    var posXPlane = xPlanes[Xp];
                    var negYPlane = yPlanes[Yn];
                    var posYPlane = yPlanes[Yp];
                    var negZPlane = zPlanes[Zn];
                    var posZPlane = zPlanes[Zp];

                    var x1 = ray.TryIntersect(negXPlane, out var negXP) && negXP.Y >= negYPlane.D && negXP.Y <= posYPlane.D && negXP.Z >= negZPlane.D && negXP.Z <= posZPlane.D;
                    var x2 = ray.TryIntersect(posXPlane, out var posXP) && posXP.Y >= negYPlane.D && posXP.Y <= posYPlane.D && posXP.Z >= negZPlane.D && posXP.Z <= posZPlane.D;
                    var y1 = ray.TryIntersect(negYPlane, out var negYP) && negYP.X >= negXPlane.D && negYP.X <= posXPlane.D && negYP.Z >= negZPlane.D && negYP.Z <= posZPlane.D;
                    var y2 = ray.TryIntersect(posYPlane, out var posYP) && posYP.X >= negXPlane.D && posYP.X <= posXPlane.D && posYP.Z >= negZPlane.D && posYP.Z <= posZPlane.D;
                    var z1 = ray.TryIntersect(negZPlane, out var negZP) && negZP.Y >= negYPlane.D && negZP.Y <= posYPlane.D && negZP.X >= negXPlane.D && negZP.X <= posXPlane.D;
                    var z2 = ray.TryIntersect(posZPlane, out var posZP) && posZP.Y >= negYPlane.D && posZP.Y <= posYPlane.D && posZP.X >= negXPlane.D && posZP.X <= posXPlane.D;

                    return x1 || x2 || y1 || y2 || z1 || z2;
                }
            }

            private List<Plane> XPlanes = new List<Plane>();
            private List<Plane> YPlanes = new List<Plane>();
            private List<Plane> ZPlanes = new List<Plane>();
            private List<InnerBound> Bounds = new List<InnerBound>();
            private readonly MeshObject obj;
            private readonly float bias;

            public MeshBound(MeshObject obj, int slices, float bias = 0.5f)
            {
                float minX = float.PositiveInfinity, minY = float.PositiveInfinity, minZ = float.PositiveInfinity,
                    maxX = float.NegativeInfinity, maxY = float.NegativeInfinity, maxZ = float.NegativeInfinity;

                for (int i = 0; i < obj.vertices.Length; ++i)
                {
                    var v = obj.vertices[i];
                    if (v.X < minX) minX = v.X;
                    if (v.Y < minY) minY = v.Y;
                    if (v.Z < minZ) minZ = v.Z;
                    if (v.X > maxX) maxX = v.X;
                    if (v.Y > maxY) maxY = v.Y;
                    if (v.Z > maxZ) maxZ = v.Z;
                }


                XPlanes.Add(new Plane(XNormal, minX));
                XPlanes.Add(new Plane(XNormal, maxX));
                YPlanes.Add(new Plane(YNormal, minY));
                YPlanes.Add(new Plane(YNormal, maxY));
                ZPlanes.Add(new Plane(ZNormal, minZ));
                ZPlanes.Add(new Plane(ZNormal, maxZ));

                var dX = MathF.Abs(minX - maxX) / slices;
                var dY = MathF.Abs(minY - maxY) / slices;
                var dZ = MathF.Abs(minZ - maxZ) / slices;

                for (int i = 1; i < slices; ++i)
                {
                    XPlanes.Add(new Plane(XNormal, minX + i * dX));
                    YPlanes.Add(new Plane(YNormal, minY + i * dY));
                    ZPlanes.Add(new Plane(ZNormal, minZ + i * dZ));
                }

                XPlanes.Sort(PlaneComparison);
                YPlanes.Sort(PlaneComparison);
                ZPlanes.Sort(PlaneComparison);

                for (int x = 0; x < slices; ++x)
                {
                    for (int y = 0; y < slices; ++y)
                    {
                        for (int z = 0; z < slices; ++z)
                        {
                            var bound = new InnerBound
                            {
                                Xn = x,
                                Xp = x + 1,
                                Yn = y,
                                Yp = y + 1,
                                Zn = z,
                                Zp = z + 1,
                                Center = new Vector3((XPlanes[x + 1].D + XPlanes[x].D) / 2, (YPlanes[y + 1].D + YPlanes[y].D) / 2, (ZPlanes[z + 1].D + ZPlanes[z].D) / 2)
                            };
                            var triangles = new List<int>(obj.triangles.Length);

                            for (int i = 0; i < obj.triangles.Length; ++i)
                            {
                                var triangle = obj.triangles[i];
                                var v1 = obj.vertices[triangle.V1];
                                var v2 = obj.vertices[triangle.V2];
                                var v3 = obj.vertices[triangle.V3];
                                if (bound.IsIn(v1, XPlanes, YPlanes, ZPlanes, bias)
                                    || bound.IsIn(v2, XPlanes, YPlanes, ZPlanes, bias)
                                    || bound.IsIn(v3, XPlanes, YPlanes, ZPlanes, bias))
                                {
                                    triangles.Add(i);
                                }
                            }

                            bound.Triangles = triangles.ToArray();
                            Bounds.Add(bound);
                        }
                    }
                }

                this.obj = obj;
                this.bias = bias;
            }

            public int[] GetBoundedTriangles(Ray ray)
            {
                var minD = float.PositiveInfinity;
                var minBound = (InnerBound)null;

                foreach (var bound in Bounds)
                {
                    if (bound.Intersects(ray, XPlanes, YPlanes, ZPlanes))
                    {
                        var d = (bound.Center - ray.StartPoint).Length();
                        if (d < minD)
                        {
                            minD = d;
                            minBound = bound;
                        }
                    }
                }

                return minBound?.Triangles;
            }

            private static int PlaneComparison(Plane a, Plane b) => (a.D - b.D) switch { var x when x < 0 => -1, var x when x > 0 => 1, _ => 0 };
        }

        private readonly Vector3[] vertices;
        private (int V1, int V2, int V3)[] triangles;
        private Vector3[] normals;
        private MeshBound bound;

        public IMaterial Material { get; }

        public Vector3 Center { get; private set; } = new Vector3(0, 0, 0);

        public MeshObject(Vector3[] vertices, (int V1, int V2, int V3)[] triangles, Vector3[] normals, IMaterial material)
        {
            this.vertices = vertices;
            this.triangles = triangles;
            this.normals = normals;
            Material = material;
        }

        public void Bound(int slices = 1)
        {
            bound = slices == 0 ? null : new MeshBound(this, slices, .5f);
        }

        public void Move(Vector3 moveVector)
        {
            Center += moveVector;

            for (int i = 0; i < vertices.Length; ++i)
            {
                vertices[i] += moveVector;
            }
            bound = null;
        }

        public void Rotate(float x, float y, float z)
        {
            var rotX = Matrix4x4.CreateRotationX(x);
            var rotY = Matrix4x4.CreateRotationY(y);
            var rotZ = Matrix4x4.CreateRotationZ(z);

            for (int i = 0; i < vertices.Length; ++i)
            {
                vertices[i] = Vector3.Transform(vertices[i], rotX);
                vertices[i] = Vector3.Transform(vertices[i], rotY);
                vertices[i] = Vector3.Transform(vertices[i], rotZ);
            }

            for (int i = 0; i < normals.Length; ++i)
            {
                normals[i] = Vector3.Transform(normals[i], rotX);
                normals[i] = Vector3.Transform(normals[i], rotY);
                normals[i] = Vector3.Transform(normals[i], rotZ);
            }

            bound = null;
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
            for (int i = 0; i < vertices.Length; ++i)
            {
                vertices[i].X = vertices[i].X * x;
                vertices[i].Y = vertices[i].Y * y;
                vertices[i].Z = vertices[i].Z * z;
            }
            bound = null;
        }

        public void Normalize()
        {
            var maxX = vertices.Select(v => v.X).Max();
            var maxY = vertices.Select(v => v.Y).Max();
            var maxZ = vertices.Select(v => v.Z).Max();

            for (int i = 0; i < vertices.Length; ++i)
            {
                vertices[i].X = vertices[i].X / maxX;
                vertices[i].Y = vertices[i].Y / maxY;
                vertices[i].Z = vertices[i].Z / maxZ;
            }
            bound = null;
        }

        public bool TryGetIntersection(Ray ray, out Intersection intersection)
        {
            intersection = default;
            intersection.Distance = float.PositiveInfinity;
            var intersects = false;

            var currentTriangles = Enumerable.Range(0, triangles.Length).ToArray();
            var boundedTriangles = (int[])null;

            if (bound != null && (boundedTriangles = bound.GetBoundedTriangles(ray)) == null)
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
                var triangle = triangles[triangleIndex];
                var v1 = vertices[triangle.V1];
                var v2 = vertices[triangle.V2];
                var v3 = vertices[triangle.V3];
                var n = normals[triangleIndex];

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
