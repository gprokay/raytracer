using System.Drawing;

namespace RayTracer.Lib
{

    public class DiffuseSolidMaterial : IMaterial
    {
        private readonly SolidBaseMaterial baseMaterial;

        public ISurfaceShader SurfaceShader { get; }

        public DiffuseSolidMaterial(Color color)
        {
            baseMaterial = new SolidBaseMaterial(color);
            SurfaceShader = new DiffuseShader();
        }

        public ILightSource AlterLight(ILightSource lightSource)
        {
            return lightSource.AlterBrightness(0);
        }

        public Light AlterLight(Light light)
        {
            return baseMaterial.AlterLight(light);
        }

        public virtual Color GetColor(TraceContext context)
        {
            var color = baseMaterial.GetColor(context.Ray, context.Intersection, context.TraceFunc, context.Depth);
            return color;
        }
    }
}
