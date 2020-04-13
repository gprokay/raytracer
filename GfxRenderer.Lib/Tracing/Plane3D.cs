using System.Numerics;

namespace GfxRenderer.Lib
{
    // (p-p0) . n = 0

    public class Plane3D
    {
        private readonly Vector3 normal;
        private readonly Vector3 origin;
        private readonly Vector3 vx;
        private readonly Vector3 vy;

        public Plane3D(Vector3 origin, Vector3 vx, Vector3 vy)
        {
            normal = Vector3.Normalize(Vector3.Cross(vx, vy));
            this.origin = origin;
            this.vx = Vector3.Normalize(vx);
            this.vy = Vector3.Normalize(vy);
        }

        public Vector2 Get2DVectors(Vector3 point)
        {
            var v = point - origin;
            var x = Vector3.Dot(vx, v);
            var y = Vector3.Dot(vy, v);
            return new Vector2(x, y);
        }

        public bool TryGet2DVectors(Ray ray, out Vector2 point)
        {
            if (ray.TryIntersectPlane(normal, origin, out var p))
            {
                point = Get2DVectors(p);
                return true;
            }

            point = default;
            return false;
        }
    }
}
