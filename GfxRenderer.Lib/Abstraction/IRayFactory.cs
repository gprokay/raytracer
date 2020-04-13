using System.Drawing;
using System.Numerics;

namespace GfxRenderer.Lib
{
    public interface IRayFactory
    {
        int Width { get; }
        int Height { get; }

        Ray GetCameraRay(int x, int y);
        Point GetPlaneIntersectionPoint(Vector3 v);
        void ProjectToZBuffer(Vector3 v1, Vector3 v2, Vector3 v3, Vector3 n, ZBufferItem[] zbuffer, MeshObject obj);
    }
}
