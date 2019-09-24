using System.Drawing;

namespace RayTracer.Lib
{
    public class OpaqueMaterial : IMaterial
    {
        private readonly OpaqueBaseMaterial opaqueObject;
        private readonly ReflectiveBaseMaterial reflectiveBaseMaterial;
        private readonly float opacity;
        
        public ISurfaceShader SurfaceShader { get; } = new DiffuseShader();

        public OpaqueMaterial(float index, Color color, float opacity, bool reflects = false)
        {
            opaqueObject = new OpaqueBaseMaterial(index, color, opacity);
            if (reflects)
            {
                reflectiveBaseMaterial = new ReflectiveBaseMaterial();
            }

            this.opacity = opacity;
        }

        public Light AlterLight(Light light)
        {
            return new Light { Brightness = light.Brightness * opacity };
        }

        public Color GetColor(TraceContext context)
        {
            var color = opaqueObject.GetColor(context.Ray, context.Intersection, context.TraceFunc, context.Depth);

            if (reflectiveBaseMaterial != null && context.IsSurface)
            {
                var reflectColor = reflectiveBaseMaterial.GetColor(context.Ray, context.Intersection, context.TraceFunc, context.Depth);
                color = color.Mix(reflectColor, .5f);
            }

            return color;
        }
    }
}
