using System;
using System.Drawing;
using System.Numerics;

namespace RayTracer.Lib
{

    public class ReflectiveBaseMaterial : IBaseMaterial
    {
        public Color GetColor(Ray ray, Intersection intersection, Func<Ray, int, Color> traceFunc, int depth)
        {
            var reflectionVector = Vector3.Reflect(ray.Direction, intersection.NormalVector);
            var reflectionRay = new Ray(intersection.IntersectionPoint, reflectionVector);
            var reflectedColor = traceFunc(reflectionRay, depth + 1);
            return reflectedColor;
        }
    }
}
