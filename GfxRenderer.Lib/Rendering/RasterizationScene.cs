using System;
using System.Collections.Generic;
using System.Drawing;
using System.Numerics;

namespace GfxRenderer.Lib
{

    public class RasterizationScene
    {
        private readonly Color backgroundColor;
        public float AmbientBrightness { get; }
        public List<MeshObject> Objects { get; } = new List<MeshObject>();
        public List<ILightSource> LightSources { get; } = new List<ILightSource>();

        public RasterizationScene(Color backgroundColor, float ambientBrightness)
        {
            this.backgroundColor = backgroundColor;
            AmbientBrightness = ambientBrightness;
        }

        public void Render(ICamera camera, int width, int height, int[] frameBuffer, ZBufferItem[] zBuffer)
        {
            var rayFactory = camera.GetRayFactory(width, height);
            
            ClearZBuffer(zBuffer);
            ProjectToZBuffer(camera, zBuffer, rayFactory);
            WriteToFrameBuffer(frameBuffer, zBuffer);
        }

        private void WriteToFrameBuffer(int[] frameBuffer, ZBufferItem[] zBuffer)
        {
            for (var i = 0; i < zBuffer.Length; ++i)
            {
                var bufferItem = zBuffer[i];
                if (bufferItem.Object == null)
                {
                    frameBuffer[i] = backgroundColor.ToArgb();
                }
                else
                {
                    var hitObject = bufferItem.Object;
                    var hitPoint = bufferItem.GetIntersection();
                    var light = GetLight(hitObject, hitPoint);

                    frameBuffer[i] = hitObject.Material.SurfaceShader
                        .Shade(
                            bufferItem.Object.Material.Color,
                            Math.Max(light.Brightness, AmbientBrightness)
                        ).ToArgb();
                }
            }
        }

        private Light GetLight(MeshObject hitObject, Intersection hitPoint)
        {
            var light = new Light { Brightness = AmbientBrightness };
            for (var j = 0; j < LightSources.Count; ++j)
            {
                var lightSource = LightSources[j];
                var shadowRay = lightSource.GetShadowRay(hitPoint.IntersectionPoint);
                var currentLight = new Light { Brightness = lightSource.Brightness };
                hitObject.Material.SurfaceShader.TryGetBrigthness(currentLight, hitPoint, shadowRay, out var shadedBrigthness);
                currentLight.Brightness = shadedBrigthness;

                if (currentLight.Brightness > light.Brightness)
                {
                    light = currentLight;
                }
            }

            return light;
        }

        private void ProjectToZBuffer(ICamera camera, ZBufferItem[] zBuffer, IRayFactory rayFactory)
        {
            foreach (var o in Objects)
            {
                for (int i = 0; i < o.Mesh.Triangles.Length; ++i)
                {
                    var t = o.Mesh.Triangles[i];
                    var v1 = o.Mesh.Vertices[t.V1];
                    var v2 = o.Mesh.Vertices[t.V2];
                    var v3 = o.Mesh.Vertices[t.V3];
                    var n = o.Mesh.Normals[i];

                    camera.ProjectToZBuffer(v1, v2, v3, n, zBuffer, o, i, rayFactory);
                }
            }
        }

        private static void ClearZBuffer(ZBufferItem[] zBuffer)
        {
            for (int z = 0; z < zBuffer.Length; ++z)
            {
                zBuffer[z] = new ZBufferItem { Distance = float.PositiveInfinity, Object = null, TriangleIndex = -1 };
            }
        }
    }
}
