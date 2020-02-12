using System.Linq;
using System.Numerics;

namespace GfxRenderer.Lib
{
    public class Mesh
    {
        public Vector3 Center { get; set; }
        public Vector3[] Vertices { get; private set; }
        public Triangle[] Triangles { get; private set; }
        public Vector3[] Normals { get; private set; }
        internal MeshBound Bound { get; set; }

        public Mesh(Vector3[] vertices, Triangle[] triangles, Vector3[] normals)
        {
            Vertices = vertices;
            Triangles = triangles;
            Normals = normals;
        }

        public void CalculateBounds(int slices = 1, float bias = 0.001f)
        {
            Bound = slices == 0 ? null : new MeshBound(this, slices, bias);
        }

        public void Move(Vector3 moveVector)
        {
            Center += moveVector;

            for (int i = 0; i < Vertices.Length; ++i)
            {
                Vertices[i] += moveVector;
            }

            Bound = null;
        }

        public void Rotate(float x, float y, float z)
        {
            var rotX = Matrix4x4.CreateRotationX(x);
            var rotY = Matrix4x4.CreateRotationY(y);
            var rotZ = Matrix4x4.CreateRotationZ(z);

            for (int i = 0; i < Vertices.Length; ++i)
            {
                Vertices[i] = Vector3.Transform(Vertices[i], rotX);
                Vertices[i] = Vector3.Transform(Vertices[i], rotY);
                Vertices[i] = Vector3.Transform(Vertices[i], rotZ);
            }

            for (int i = 0; i < Normals.Length; ++i)
            {
                Normals[i] = Vector3.Transform(Normals[i], rotX);
                Normals[i] = Vector3.Transform(Normals[i], rotY);
                Normals[i] = Vector3.Transform(Normals[i], rotZ);
            }

            Bound = null;
        }

        public void Scale(float x, float y, float z)
        {
            for (int i = 0; i < Vertices.Length; ++i)
            {
                Vertices[i].X = Vertices[i].X * x;
                Vertices[i].Y = Vertices[i].Y * y;
                Vertices[i].Z = Vertices[i].Z * z;
            }
            Bound = null;
        }

        public void Normalize()
        {
            var maxX = Vertices.Select(v => v.X).Max();
            var maxY = Vertices.Select(v => v.Y).Max();
            var maxZ = Vertices.Select(v => v.Z).Max();

            for (int i = 0; i < Vertices.Length; ++i)
            {
                Vertices[i].X = Vertices[i].X / maxX;
                Vertices[i].Y = Vertices[i].Y / maxY;
                Vertices[i].Z = Vertices[i].Z / maxZ;
            }
            Bound = null;
        }
    }
}
