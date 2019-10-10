using System;
using System.Drawing;
using System.Numerics;

namespace RayTracer.Lib
{
    public class OpaqueBaseMaterial : IBaseMaterial
    {
        private readonly float refractionIndex;
        private readonly Color color;
        private readonly float opacity;

        public OpaqueBaseMaterial(float refractionIndex, Color color, float opacity)
        {
            this.refractionIndex = refractionIndex;
            this.color = color;
            this.opacity = opacity;
        }

        public Light AlterLight(Light light)
        {
            return new Light { Brightness = light.Brightness * opacity };
        }

        public Color GetColor(Ray ray, Intersection intersection, Func<Ray, int, Color> traceFunc, int depth)
        {
            var cosi = Vector3.Dot(ray.Direction, intersection.NormalVector);
            var etat = refractionIndex;
            var etai = 1f;
            if (cosi > 1) cosi = 1;
            if (cosi < -1) cosi = -1;
            var normal = intersection.NormalVector;
            if (cosi < 0)
            {
                cosi = -1 * cosi;
            }
            else
            {
                normal = -1 * normal;
                var h = etai;
                etai = etat;
                etat = h;
            }

            var eta = etai / etat;
            var k = 1 - eta * eta * (1 - cosi * cosi);

            if (k < 0)
                return color;

            var v = eta * ray.Direction + (eta * cosi - (float)Math.Sqrt(k)) * normal;
            var refractionRay = new Ray(intersection.IntersectionPoint, v);
            return traceFunc(refractionRay, depth + 1).Mix(color, 1 - opacity);
        }
    }
}
