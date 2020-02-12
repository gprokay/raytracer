using System;
using System.Drawing;

namespace GfxRenderer.Lib
{
    public class SolidBaseMaterial : IBaseMaterial
    {
        public Color Color { get; }

        public SolidBaseMaterial(Color color)
        {
            Color = color;
        }

        public Color GetColor(Ray ray, Intersection intersection, Func<Ray, int, Color> traceFunc, int depth)
        {
            return Color;
        }

        public Light AlterLight(Light light)
        {
            return new Light { Brightness = 0 };
        }
    }
}
