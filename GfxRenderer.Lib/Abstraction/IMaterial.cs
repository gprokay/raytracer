using System.Drawing;

namespace GfxRenderer.Lib
{
    public interface IMaterial
    {
        Color Color { get; }

        ISurfaceShader SurfaceShader { get; }

        Color GetColor(TraceContext context);

        Light AlterLight(Light light);
    }
}
