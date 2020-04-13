using System.Drawing;

namespace GfxRenderer.Lib
{
    public class OpaqueMaterial : IMaterial
    {
        private readonly OpaqueBaseMaterial opaqueObject;
        private readonly ReflectiveBaseMaterial reflectiveBaseMaterial;
        private readonly float? reflectivity;

        public Color Color { get; private set; }

        public ISurfaceShader SurfaceShader { get; } = new DiffuseShader();

        public OpaqueMaterial(float index, Color color, float opacity, float? reflectivity = null)
        {
            Color = color;
            this.reflectivity = reflectivity;
            opaqueObject = new OpaqueBaseMaterial(index, color, opacity);

            if (reflectivity.HasValue)
            {
                reflectiveBaseMaterial = new ReflectiveBaseMaterial();
            }
        }

        public Light AlterLight(Light light)
        {
            return opaqueObject.AlterLight(light);
        }

        public Color GetColor(TraceContext context)
        {
            var color = opaqueObject.GetColor(context.Ray, context.Intersection, context.TraceFunc, context.Depth);

            if (reflectiveBaseMaterial != null && context.IsSurface)
            {
                var reflectColor = reflectiveBaseMaterial.GetColor(context.Ray, context.Intersection, context.TraceFunc, context.Depth);
                color = color.Mix(reflectColor, reflectivity.Value);
            }

            return color;
        }
    }
}
