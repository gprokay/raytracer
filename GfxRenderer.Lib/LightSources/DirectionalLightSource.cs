using System.Numerics;

namespace GfxRenderer.Lib
{
    public class DirectionalLightSource : ILightSource
    {
        public Vector3 Direction { get; }
        public float Brightness { get; }

        public DirectionalLightSource(Vector3 direction, float brightness)
        {
            Direction = direction;
            Brightness = brightness;
        }

        public Ray GetShadowRay(Vector3 targetPoint)
        {
            return new Ray(targetPoint, Direction);
        }
    }
}
