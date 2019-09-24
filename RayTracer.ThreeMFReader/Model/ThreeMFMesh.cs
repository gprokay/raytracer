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
        public Vector3[] Vectors { get; set; }

        public Triangle[] Triangles { get; set; }
    }
}
