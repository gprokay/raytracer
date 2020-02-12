namespace GfxRenderer.Lib
{

    public interface IRenderObject
    {
        bool TryGetIntersection(Ray ray, out Intersection intersection);

        IMaterial Material { get; }
    }
}
