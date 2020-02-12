using System.Drawing;
using System.Numerics;

namespace RayTracer.Lib
{
    public interface IRayFactory
    {
        int Width { get; }
        int Height { get; }

        Ray GetCameraRay(int x, int y);
        Point GetPlaneIntersectionPoint(Vector3 v);
    }
}
