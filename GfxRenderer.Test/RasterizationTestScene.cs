using GfxRenderer.Lib;
using GfxRenderer.ThreeMFReader;
using System;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Numerics;
using System.Threading;
using Triangle = GfxRenderer.Lib.Triangle;

namespace GfxRenderer.Test
{
    public class RasterizationTestScene
    {
        public MeshObject Sphere { get; private set; }
        public ICamera Camera { get; private set; }
        public RasterizationScene Scene { get; private set; }
        public ILightSource LightSource { get; set; }
        public MeshObject Cube { get; private set; }
        public Vector3 SphereCenter { get; set; }
        private ZBufferItem[] ZBuffer { get; set; }

        public RasterizationTestScene(int meshcomplexity = 6)
        {
            var meshes = ThreeMFLoader.LoadFromFile(@".\data\test.3mf").ToList();
            var mesh = meshes[0];
            Cube = new MeshObject(new Mesh(mesh.Vertices, mesh.Triangles.Select(t => new Triangle(t.Vector1, t.Vector2, t.Vector3)).ToArray(), mesh.Normals), new OpaqueMaterial(1.2f, Color.Red, .8f, true));
            Cube.Mesh.Normalize();
            Cube.Mesh.Rotate(MathF.PI / 4, MathF.PI / 4, 0);

            var sphereMesh = ProceduralSphere.GetSphereMesh(1f, meshcomplexity);
            SphereCenter = new Vector3(0, 0, 3.5f);
            Sphere = new MeshObject(sphereMesh, new OpaqueMaterial(1.2f, Color.Yellow, .8f, true));
            Sphere.Mesh.Move(SphereCenter);
            //Sphere.Mesh.CalculateBounds(1);

            Camera = new RectCamera(new Vector3(-16 / 4 / 2f, -9 / 4 / 2f, -6), new Vector3(16 / 4f, 0, 0), new Vector3(0, 9 / 4f, 0), -4f);
            Scene = new RasterizationScene(Color.BlueViolet, .3f);
            LightSource = new DirectionalLightSource(new Vector3(-1, 1, 1), 1f);

            Scene.LightSources.Add(LightSource);
            Scene.Objects.Add(Cube);
            Scene.Objects.Add(Sphere);
        }

        public void ReplaceMesh(int factor = 6)
        {
            var sphereMesh = ProceduralSphere.GetSphereMesh(1f, 6);
            SphereCenter = new Vector3(0, 0, 3.5f);
            Sphere = new MeshObject(sphereMesh, new OpaqueMaterial(1.2f, Color.Yellow, .8f, true));
            Sphere.Mesh.Move(SphereCenter);
            Scene.Objects[1] = Sphere;
        }

        public string RenderScene(int[] colors, int width, int height, bool parallel = true, CancellationToken? cancellationToken = null)
        {
            if (ZBuffer == null)
            {
                ZBuffer = new ZBufferItem[width * height];
            }
            var watch = Stopwatch.StartNew();
            Scene.Render(Camera, width, height, colors, ZBuffer);
            watch.Stop();
            var tCount = Sphere.Mesh.Triangles.Length + Cube.Mesh.Triangles.Length;
            var t = (int)(tCount / (float)watch.ElapsedMilliseconds);
            var fps = 1000 / watch.ElapsedMilliseconds;
            var debug = "Rendered in " + watch.ElapsedMilliseconds + " ms Polycount: " + tCount + "; Speed: " + t + " t/ms; FPS: " + fps;
            Debug.WriteLine(debug);
            return debug;
        }
    }
}
