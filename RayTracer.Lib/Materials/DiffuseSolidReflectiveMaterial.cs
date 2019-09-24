using System.Drawing;

namespace RayTracer.Lib
{
    public class DiffuseSolidReflectiveMaterial : IMaterial
    {
        private readonly ReflectiveBaseMaterial reflectiveMaterial = new ReflectiveBaseMaterial();
        private readonly SolidBaseMaterial solidMaterial;
        public ISurfaceShader SurfaceShader { get; } = new DiffuseShader();

        public DiffuseSolidReflectiveMaterial(Color color)
        {
            solidMaterial = new SolidBaseMaterial(color);
        }

        public Light AlterLight(Light light)
        {
            return new Light { Brightness = 0 };
        }

        public Color GetColor(TraceContext context)
        {
            var color = reflectiveMaterial.GetColor(context.Ray, context.Intersection, context.TraceFunc, context.Depth)
                .Mix(solidMaterial.GetColor(context.Ray, context.Intersection, context.TraceFunc, context.Depth), .5f);

            return color;
        }
    }
}
