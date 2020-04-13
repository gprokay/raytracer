
using System.Diagnostics;
using System.Numerics;

namespace GfxRenderer.Lib
{
    [DebuggerDisplay("P= {IntersectionPoint}; D= {Distance}; N= {NormalVector}")]
    public struct Intersection
    {
        public Vector3 IntersectionPoint;
        public Vector3 NormalVector;
        public float Distance;

        public Intersection(Vector3 intersectionPoint, Vector3 normalVector, float distance)
        {
            IntersectionPoint = intersectionPoint;
            NormalVector = Vector3.Normalize(normalVector);
            Distance = distance;
        }
    }
}
