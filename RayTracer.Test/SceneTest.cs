using RayTracer.Lib;
using RayTracer.ThreeMFReader;
using System;
using System.Drawing;
using System.Linq;
using System.Numerics;
using System.Threading;

namespace RayTracer.Test
{
    public class SceneTest
    {
        public static SphereObject Sphere { get; private set; }
        public static ICamera Camera { get; private set; }
        public static RayTracerScene Scene { get; private set; }
        public static SphericalLightSource LightSource { get; set; }
        public static MeshObject MeshObj { get; private set; }

        static SceneTest()
        {
            var meshes = ThreeMFLoader.LoadFromFile(@".\data\test.3mf").ToList();
            var mesh = meshes[0];

            MeshObj = new MeshObject(mesh.Vectors, mesh.Triangles.Select(t => (t.Vector1, t.Vector2, t.Vector3)).ToArray(), new OpaqueMaterial(1.2f, Color.Red, .8f));
            MeshObj.Normalize();
            MeshObj.Rotate(MathF.PI / 4, MathF.PI / 4, 0);

            Sphere = new SphereObject(new Vector3(0, 0, 3.5f), 1.5f, new OpaqueMaterial(1.2f, Color.Yellow, .8f, true));
            Camera = new RectCamera(new Vector3(-16 / 4 / 2f, -9 / 4 / 2f, -5), new Vector3(16 / 4f, 0, 0), new Vector3(0, 9 / 4f, 0), -4f);
            Scene = new RayTracerScene(Color.DarkCyan, .3f);
            LightSource = new SphericalLightSource(new Vector3(5, -5, -5), 1f);

            Scene.LightSources.Add(LightSource);
            Scene.Objects.Add(MeshObj);
            Scene.Objects.Add(Sphere);
        }

        public static Color[] RenderScene(int width, int height, bool parallel = true, CancellationToken? cancellationToken = null)
        {
            var result = new Color[width * height];
            Scene.Render(Camera, result, width, height, parallel, cancellationToken);
            return result;
        }

    }
}
