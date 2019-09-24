using System.Drawing;

namespace RayTracer.Lib
{
    public static class ColorExtensions
    {
        public static Color Mix(this Color color, Color other, float mixOther)
        {
            return Color.FromArgb(
                color.A,
                MaxAB(color.R, other.R, mixOther),
                MaxAB(color.G, other.G, mixOther),
                MaxAB(color.B, other.B, mixOther));
        }

        private static int MaxAB(int a, int b, float mixB)
        {
            var d = b - a;
            return (int)(d * mixB) + a;
        }
    }
}
