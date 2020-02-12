using System.Drawing;

namespace GfxRenderer.Lib
{
    public class DiffuseSolidReflectiveMaterial : IMaterial
    {
        private readonly ReflectiveBaseMaterial reflectiveMaterial = new ReflectiveBaseMaterial();
        private readonly SolidBaseMaterial solidMaterial;
        public ISurfaceShader SurfaceShader { get; } = new DiffuseShader();
        public Color Color { get; private set; }

        public DiffuseSolidReflectiveMaterial(Color color)
        {
            Color = color;
            solidMaterial = new SolidBaseMaterial(color);
        }

        public Light AlterLight(Light light)
        {
            return solidMaterial.AlterLight(light);
        }

        public Color GetColor(TraceContext context)
        {
            var color = reflectiveMaterial.GetColor(context.Ray, context.Intersection, context.TraceFunc, context.Depth)
                .Mix(solidMaterial.GetColor(context.Ray, context.Intersection, context.TraceFunc, context.Depth), .5f);

            return color;
        }
    }
}
