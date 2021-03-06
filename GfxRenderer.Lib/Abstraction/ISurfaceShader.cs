﻿using System.Drawing;

namespace GfxRenderer.Lib
{
    public interface ISurfaceShader
    {
        bool TryGetBrigthness(Light light, Intersection intersection, Ray shadowRay, out float brigthness);

        Color Shade(Color color, float brigthness);
    }
}
