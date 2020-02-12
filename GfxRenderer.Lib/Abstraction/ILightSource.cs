using System.Numerics;

namespace GfxRenderer.Lib
{

    public interface ILightSource
    {
        float Brightness { get; }

        Ray GetShadowRay(Vector3 targetPoint);
    }
}
