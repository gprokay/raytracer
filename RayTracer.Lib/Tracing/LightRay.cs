﻿namespace RayTracer.Lib
{
    public struct LightRay
    {
        public Light Light;

        public Ray ShadowRay;

        public bool IsAmbient;
    }
}
