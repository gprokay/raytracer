namespace GfxRenderer.Lib
{
    public struct ZBufferItem 
    {
        public MeshObject Object;
        public Intersection Intersection;
        public float u;
        public float v;
    }


    public interface ICamera
    {
        IRayFactory GetRayFactory(int pixelWidth, int pixelHeight);
    }
}
