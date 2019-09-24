using System;
using System.Collections.Generic;
using System.Drawing;
using System.Numerics;
using System.Threading;
using System.Threading.Tasks;

namespace RayTracer.Lib
{
    public class RayTracerScene
    {
        public List<IRayTraceObject> Objects { get; } = new List<IRayTraceObject>();
        public List<ILightSource> LightSources { get; } = new List<ILightSource>();
        public Color BackgroundColor { get; }
        public float AmbientBrightness { get; }

        public RayTracerScene(Color backgroundColor, float ambientBrightness = 0)
        {
            BackgroundColor = backgroundColor;
            AmbientBrightness = ambientBrightness;
        }

        public void Render(ICamera camera, Color[] colors, int width, int height, bool parallel = true, CancellationToken? cancellationToken = null)
        {
            var rayFactory = camera.GetRayFactory(width, height);
            if (parallel)
            {
                Parallel.For(0, height, y =>
                {
                    for (var x = 0; x < width; ++x)
                    {
                        if (cancellationToken.HasValue && cancellationToken.Value.IsCancellationRequested) return;
                        var index = y * width + x;
                        var ray = rayFactory.GetCameraRay(x, y);
                        colors[index] = Trace(ray, 0);
                    }
                });
            }
            else
            {
                for (var x = 0; x < width; ++x)
                {
                    for (var y = 0; y < height; ++y)
                    {
                        if (cancellationToken.HasValue && cancellationToken.Value.IsCancellationRequested) return;
                        var index = y * width + x;
                        var ray = rayFactory.GetCameraRay(x, y);
                        colors[index] = Trace(ray, 0);
                    }
                }
            }
        }

        private Color Trace(Ray ray, int depth)
        {
            if (depth > 3) return BackgroundColor;
            var hitObject = (IRayTraceObject)null;
            var hitPoint = (Intersection)(default);

            foreach (var obj in Objects)
            {
                if (!obj.TryGetIntersection(ray, out var intersection))
                {
                    continue;
                }

                if (hitObject == null || intersection.Distance < hitPoint.Distance)
                {
                    hitPoint = intersection;
                    hitObject = obj;
                }
            }

            if (hitObject == null) return BackgroundColor;

            LightRay lightRay = new LightRay { Light = new Light { Brightness = AmbientBrightness }, IsAmbient = true };
            foreach (var lightSource in LightSources)
            {
                var shadowRay = lightSource.GetShadowRay(hitPoint.IntersectionPoint);

                var currentLight = new Light { Brightness = lightSource.Brightness };

                foreach (var obj in Objects)
                {
                    if (obj.TryGetIntersection(shadowRay, out _) && currentLight.Brightness > AmbientBrightness)
                    {
                        currentLight = obj.Material.AlterLight(currentLight);
                    }
                }

                var dotProduct = Vector3.Dot(shadowRay.Direction, hitPoint.NormalVector) * -1;

                var needShading = hitObject.Material.SurfaceShader.TryGetBrigthness(currentLight, hitPoint, shadowRay, out var shadedBrigthness);
                if (needShading)
                {
                    currentLight.Brightness = shadedBrigthness;
                }

                if (currentLight.Brightness > lightRay.Light.Brightness && dotProduct > 0)
                {
                    lightRay.Light = currentLight;
                    lightRay.ShadowRay = shadowRay;
                    lightRay.IsAmbient = false;
                }
            }

            var dp = Vector3.Dot(ray.Direction, hitPoint.NormalVector) * -1;
            var isSurface = dp > 0;

            var color = hitObject.Material.GetColor(new TraceContext
            {
                Ray = ray,
                Intersection = hitPoint,
                IsSurface = isSurface,
                TraceFunc = Trace,
                Depth = depth
            });

            return isSurface
                ? hitObject.Material.SurfaceShader.Shade(color, Math.Max(lightRay.Light.Brightness, AmbientBrightness))
                : color;
        }
    }
}
