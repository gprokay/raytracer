using System;
using System.Drawing;

namespace GfxRenderer.Lib
{
    public interface IBaseMaterial
    {
        Color GetColor(Ray ray, Intersection intersection, Func<Ray, int, Color> traceFunc, int depth);

        Light AlterLight(Light light);
    }
}
