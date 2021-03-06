﻿using GfxRenderer.Lib;
using GfxRenderer.ThreeMFReader;
using System;
using System.Drawing;
using System.Linq;
using System.Numerics;

namespace GfxRenderer.Test
{
    public static class AppIconScene
    {
        public static Bitmap RenderIconBitmap(int size)
        {
            var sphere = new SphereObject(new Vector3(0, 0, 0), 1, new DiffuseSolidMaterial(Color.Yellow));
            var light = new SphericalLightSource(new Vector3(3, -3, -3), 1f);
            var camera = new RectCamera(new Vector3(-1, -1, -4), new Vector3(2, 0, 0), new Vector3(0, 2, 0), -4f);

            var scene = new RayTracerScene(Color.Green);
            scene.LightSources.Add(light);
            scene.Objects.Add(sphere);

            var colors = new int[size * size];

            scene.Render(camera, colors, size, size, true);
            
            var bitmap = new DirectBitmap(size, size);
            bitmap.SetPixels(colors);

            return bitmap.Bitmap;
        }
    }
}
