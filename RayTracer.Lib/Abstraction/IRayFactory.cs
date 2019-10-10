namespace RayTracer.Lib
{
    public interface IRayFactory
    {
        Ray GetCameraRay(int x, int y);
    }
}
