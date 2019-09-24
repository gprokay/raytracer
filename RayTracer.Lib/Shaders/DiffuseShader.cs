using System;
using System.Drawing;
using System.Numerics;

namespace RayTracer.Lib
{
    public class DiffuseShader : ISurfaceShader
    {
        public bool TryGetBrigthness(Light light, Intersection intersection, Ray shadowRay, out float brigthness)
        {
            var dot = Vector3.Dot(shadowRay.Direction, intersection.NormalVector) * -1;
            if (dot < 0)
            {
                brigthness = 0;
                return false;
            }
            else
            {
                brigthness = dot / (shadowRay.Direction.Length() * intersection.NormalVector.Length()) * light.Brightness;
                return true;
            }
        }

        public Color Shade(Color color, float brigthness)
        {
            return Color.FromArgb((int)(brigthness * color.R), (int)(brigthness * color.G), (int)(brigthness * color.B));
        }
    }
}
