using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;

namespace RayTracer.Lib
{
    internal class MeshBound
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

                var x1 = ray.TryIntersectPlane(negXPlane, out var negXP) && negXP.Y >= negYPlane.D && negXP.Y <= posYPlane.D && negXP.Z >= negZPlane.D && negXP.Z <= posZPlane.D;
                var x2 = ray.TryIntersectPlane(posXPlane, out var posXP) && posXP.Y >= negYPlane.D && posXP.Y <= posYPlane.D && posXP.Z >= negZPlane.D && posXP.Z <= posZPlane.D;
                var y1 = ray.TryIntersectPlane(negYPlane, out var negYP) && negYP.X >= negXPlane.D && negYP.X <= posXPlane.D && negYP.Z >= negZPlane.D && negYP.Z <= posZPlane.D;
                var y2 = ray.TryIntersectPlane(posYPlane, out var posYP) && posYP.X >= negXPlane.D && posYP.X <= posXPlane.D && posYP.Z >= negZPlane.D && posYP.Z <= posZPlane.D;
                var z1 = ray.TryIntersectPlane(negZPlane, out var negZP) && negZP.Y >= negYPlane.D && negZP.Y <= posYPlane.D && negZP.X >= negXPlane.D && negZP.X <= posXPlane.D;
                var z2 = ray.TryIntersectPlane(posZPlane, out var posZP) && posZP.Y >= negYPlane.D && posZP.Y <= posYPlane.D && posZP.X >= negXPlane.D && posZP.X <= posXPlane.D;

                return x1 || x2 || y1 || y2 || z1 || z2;
            }
        }

        private List<Plane> XPlanes = new List<Plane>();
        private List<Plane> YPlanes = new List<Plane>();
        private List<Plane> ZPlanes = new List<Plane>();
        private List<InnerBound> Bounds = new List<InnerBound>();
        private readonly Mesh obj;

        public MeshBound(Mesh obj, int slices, float bias = 0f)
        {
            float minX = float.PositiveInfinity, minY = float.PositiveInfinity, minZ = float.PositiveInfinity,
                maxX = float.NegativeInfinity, maxY = float.NegativeInfinity, maxZ = float.NegativeInfinity;

            for (int i = 0; i < obj.Vertices.Length; ++i)
            {
                var v = obj.Vertices[i];
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

            var index = 0;
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
                        var triangles = new List<int>(obj.Triangles.Length);

                        for (int i = 0; i < obj.Triangles.Length; ++i)
                        {
                            var triangle = obj.Triangles[i];
                            var v1 = obj.Vertices[triangle.V1];
                            var v2 = obj.Vertices[triangle.V2];
                            var v3 = obj.Vertices[triangle.V3];
                            if (bound.IsIn(v1, XPlanes, YPlanes, ZPlanes, bias)
                                || bound.IsIn(v2, XPlanes, YPlanes, ZPlanes, bias)
                                || bound.IsIn(v3, XPlanes, YPlanes, ZPlanes, bias))
                            {
                                triangles.Add(i);
                            }
                        }

                        bound.Triangles = triangles.ToArray();
                        Bounds.Add(bound);
                        index++;
                    }
                }
            }

            Debug.WriteLine(Bounds.Skip(6).Take(1).FirstOrDefault()?.Triangles.Length);
            this.obj = obj;
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
}
