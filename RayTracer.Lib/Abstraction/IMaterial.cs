using System.Drawing;

namespace RayTracer.Lib
{
    public interface IMaterial
    {
        ISurfaceShader SurfaceShader { get; }

        Color GetColor(TraceContext context);

        Light AlterLight(Light light);
    }
}
