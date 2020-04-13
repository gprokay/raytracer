namespace GfxRenderer.Lib
{

    public interface IRenderObject
    {
        IMaterial Material { get; }

        bool TryGetIntersection(Ray ray, out Intersection intersection);
    }
}
