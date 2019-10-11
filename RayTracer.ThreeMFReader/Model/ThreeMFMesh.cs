using System.Numerics;

namespace RayTracer.ThreeMFReader
{
    public struct Triangle
    {
        public int Vector1 { get; set; }
        public int Vector2 { get; set; }
        public int Vector3 { get; set; }
    }

    public class ThreeMFMesh
    {
        public Vector3[] Vertices { get; set; }

        public Triangle[] Triangles { get; set; }

        public Vector3[] Normals { get; set; }
    }
}
