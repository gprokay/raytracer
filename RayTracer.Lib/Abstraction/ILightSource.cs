using System.Numerics;

namespace RayTracer.Lib
{

    public interface ILightSource
    {
        Vector3 Center { get; }

        float Brightness { get; }

        Ray GetShadowRay(Vector3 targetPoint);

        ILightSource AlterBrightness(float brightness);
    }
}
