using System.Drawing;
using System.Numerics;

namespace GfxRenderer.Lib
{
    public struct ZBufferItem
    {
        public MeshObject Object;
        public float Distance;
        public int TriangleIndex;
        public Vector3 Intersection;
        public Vector3 Normal;

        public Intersection GetIntersection()
        {
            return new Intersection
            {
                Distance = Distance,
                IntersectionPoint = Intersection,
                NormalVector = Normal
            };
        }
    }


    public interface ICamera
    {
        IRayFactory GetRayFactory(int pixelWidth, int pixelHeight);
        void ProjectToZBuffer(Vector3 v1, Vector3 v2, Vector3 v3, Vector3 n, ZBufferItem[] zbuffer, MeshObject obj, int triangleIndex, IRayFactory rayFactory);
    }
}
