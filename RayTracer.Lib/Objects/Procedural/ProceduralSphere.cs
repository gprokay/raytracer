using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace RayTracer.Lib
{
    public static class ProceduralSphere
    {
        public static Mesh GetSphereMesh(float radius = 1f, int recursionLevel = 3)
        {
            List<Vector3> vertList = new List<Vector3>();
            Dictionary<long, int> middlePointIndexCache = new Dictionary<long, int>();

            // create 12 vertices of a icosahedron
            float t = (1f + MathF.Sqrt(5f)) / 2f;

            vertList.Add(Vector3.Normalize(new Vector3(-1f, t, 0f)) * radius);
            vertList.Add(Vector3.Normalize(new Vector3(1f, t, 0f)) * radius);
            vertList.Add(Vector3.Normalize(new Vector3(-1f, -t, 0f)) * radius);
            vertList.Add(Vector3.Normalize(new Vector3(1f, -t, 0f)) * radius);

            vertList.Add(Vector3.Normalize(new Vector3(0f, -1f, t)) * radius);
            vertList.Add(Vector3.Normalize(new Vector3(0f, 1f, t)) * radius);
            vertList.Add(Vector3.Normalize(new Vector3(0f, -1f, -t)) * radius);
            vertList.Add(Vector3.Normalize(new Vector3(0f, 1f, -t)) * radius);

            vertList.Add(Vector3.Normalize(new Vector3(t, 0f, -1f)) * radius);
            vertList.Add(Vector3.Normalize(new Vector3(t, 0f, 1f)) * radius);
            vertList.Add(Vector3.Normalize(new Vector3(-t, 0f, -1f)) * radius);
            vertList.Add(Vector3.Normalize(new Vector3(-t, 0f, 1f)) * radius);


            // create 20 triangles of the icosahedron
            List<Triangle> faces = new List<Triangle>();

            // 5 faces around point 0
            faces.Add(new Triangle(0, 11, 5));
            faces.Add(new Triangle(0, 5, 1));
            faces.Add(new Triangle(0, 1, 7));
            faces.Add(new Triangle(0, 7, 10));
            faces.Add(new Triangle(0, 10, 11));

            // 5 adjacent faces 
            faces.Add(new Triangle(1, 5, 9));
            faces.Add(new Triangle(5, 11, 4));
            faces.Add(new Triangle(11, 10, 2));
            faces.Add(new Triangle(10, 7, 6));
            faces.Add(new Triangle(7, 1, 8));

            // 5 faces around point 3
            faces.Add(new Triangle(3, 9, 4));
            faces.Add(new Triangle(3, 4, 2));
            faces.Add(new Triangle(3, 2, 6));
            faces.Add(new Triangle(3, 6, 8));
            faces.Add(new Triangle(3, 8, 9));

            // 5 adjacent faces 
            faces.Add(new Triangle(4, 9, 5));
            faces.Add(new Triangle(2, 4, 11));
            faces.Add(new Triangle(6, 2, 10));
            faces.Add(new Triangle(8, 6, 7));
            faces.Add(new Triangle(9, 8, 1));


            // refine triangles
            for (int i = 0; i < recursionLevel; i++)
            {
                List<Triangle> faces2 = new List<Triangle>();
                foreach (var tri in faces)
                {
                    // replace triangle by 4 triangles
                    int a = getMiddlePoint(tri.V1, tri.V2, ref vertList, ref middlePointIndexCache, radius);
                    int b = getMiddlePoint(tri.V2, tri.V3, ref vertList, ref middlePointIndexCache, radius);
                    int c = getMiddlePoint(tri.V3, tri.V1, ref vertList, ref middlePointIndexCache, radius);

                    faces2.Add(new Triangle(tri.V1, a, c));
                    faces2.Add(new Triangle(tri.V2, b, a));
                    faces2.Add(new Triangle(tri.V3, c, b));
                    faces2.Add(new Triangle(a, b, c));
                }
                faces = faces2;
            }

            var normals = faces.Select(f =>
            {
                var v1 = vertList[f.V1];
                var v2 = vertList[f.V2];
                var v3 = vertList[f.V3];
                return Vector3.Normalize(Vector3.Cross(v2 - v1, v3 - v1));
            }).ToArray();

            return new Mesh(vertList.ToArray(), faces.ToArray(), normals);
        }

        private static int getMiddlePoint(int p1, int p2, ref List<Vector3> vertices, ref Dictionary<long, int> cache, float radius)
        {
            // first check if we have it already
            bool firstIsSmaller = p1 < p2;
            long smallerIndex = firstIsSmaller ? p1 : p2;
            long greaterIndex = firstIsSmaller ? p2 : p1;
            long key = (smallerIndex << 32) + greaterIndex;

            int ret;
            if (cache.TryGetValue(key, out ret))
            {
                return ret;
            }

            // not in cache, calculate it
            Vector3 point1 = vertices[p1];
            Vector3 point2 = vertices[p2];
            Vector3 middle = new Vector3
            (
                (point1.X + point2.X) / 2f,
                (point1.Y + point2.Y) / 2f,
                (point1.Z + point2.Z) / 2f
            );

            // add vertex makes sure point is on unit sphere
            int i = vertices.Count;
            vertices.Add(Vector3.Normalize(middle) * radius);

            // store it, return index
            cache.Add(key, i);

            return i;
        }
    }
}
