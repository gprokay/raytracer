using System.Numerics;

namespace RayTracer.Lib
{
    public interface ICamera
    {
        public Ray[] GetCameraRays(int pixelWidth, int pixelHeight);

        IRayFactory GetRayFactory(int pixelWidth, int pixelHeight);
    }
}
