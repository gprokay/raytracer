using System;
using System.Numerics;

namespace RayTracer.Lib
{
    // (x−x0)2+(y−y0)2+(z−z0)2=r2
    // |x-c|^2 = r^2

    // x=x0+ta y=y0+tb z=z0+tc
    // x = o - dl

    public class SphereObject : IRayTraceObject
    {
        public Vector3 Center { get; set; }
        public float R { get; set; }
        public IMaterial Material { get; set; }

        public SphereObject(Vector3 center, float r, IMaterial material)
        {
            Center = center;
            R = r;
            Material = material;
        }

        public bool TryGetIntersection(Ray ray, out Intersection intersection)
        {
            intersection = default;

            // var a = 1; //ray.Direction.LengthSquared();
            var b = 2 * Vector3.Dot(ray.Direction, ray.StartPoint - Center);
            var c = (ray.StartPoint - Center).LengthSquared() - (float)Math.Pow(R, 2);

            var d = Math.Pow(b, 2) - 4 * c;

            if (d < 0) { return false; }

            var sqrt = (float)Math.Sqrt(d);

            var d1 = (-1 * b + sqrt) / 2;
            var d2 = (-1 * b - sqrt) / 2;

            var p1 = ray.StartPoint + d1 * ray.Direction;
            var n1 = p1 - Center;

            var p2 = ray.StartPoint + d2 * ray.Direction;
            var n2 = p2 - Center;

            var d1Good = d1 > 0.001f && (d1 < ray.Length - 0.001f || ray.Length == float.PositiveInfinity);
            var d2Good = d2 > 0.001f && (d2 < ray.Length - 0.001f || ray.Length == float.PositiveInfinity);

            if(d1Good && d2Good)
            {
                if (d1 < d2)
                {
                    intersection = new Intersection(p1, n1, d1);
                    return true;
                }
                else
                {
                    intersection = new Intersection(p2, n2, d2);
                    return true;
                }
            }
            else if (d1Good)
            {
                intersection = new Intersection(p1, n1, d1);
                return true;
            }
            else if (d2Good)
            {
                intersection = new Intersection(p2, n2, d2);
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
