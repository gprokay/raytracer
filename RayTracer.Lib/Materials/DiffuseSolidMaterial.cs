﻿using System;
using System.Collections.Generic;
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
            return new Light { Brightness = 0 };
        }

        public virtual Color GetColor(TraceContext context)
        {
            var color = baseMaterial.GetColor(context.Ray, context.Intersection, context.TraceFunc, context.Depth);
            return color;
        }

        public Light AlterLight(Light light, Ray shadowRay)
        {
            throw new NotImplementedException();
        }
    }
}