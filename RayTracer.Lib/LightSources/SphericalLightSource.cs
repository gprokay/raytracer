using System.Numerics;

namespace RayTracer.Lib
{
    public struct SphericalLightSource : ILightSource
    {
        public Vector3 Center { get; set; }

        public float Brightness { get; }

        public SphericalLightSource(Vector3 center, float brightness)
        {
            Center = center;
            Brightness = brightness;
        }

        public Ray GetShadowRay(Vector3 targetPoint)
        {
            return new Ray(Center, targetPoint - Center, (targetPoint - Center).Length());
        }
    }
}
