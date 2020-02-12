using System.Drawing;
using System.Numerics;

namespace GfxRenderer.Lib
{
    // (p-p0) . n = 0

    public class PlaneObject : IRenderObject
    {
        private readonly Vector3 point;
        private readonly Vector3 normalVector;

        public Color Color { get; }

        public IMaterial Material { get; }

        public PlaneObject(Vector3 point, Vector3 normalVector, IMaterial material)
        {
            this.point = point;
            this.normalVector = normalVector;
            Material = material;
        }

        public bool TryGetIntersection(Ray ray, out Intersection intersection)
        {
            var d = Vector3.Dot(point - ray.StartPoint, normalVector) / Vector3.Dot(ray.Direction, normalVector);
            var p = ray.StartPoint + ray.Direction * d;
            if (d > .001f && (d < ray.Length - .001f || ray.Length == float.PositiveInfinity))
            {
                intersection = new Intersection(p, normalVector, d);
                return true;
            }

            intersection = default;
            return false;
        }
    }
}
