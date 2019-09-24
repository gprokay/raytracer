namespace RayTracer.Lib
{

    public interface IRayTraceObject
    {
        bool TryGetIntersection(Ray ray, out Intersection intersection);

        IMaterial Material { get; }
    }
}
