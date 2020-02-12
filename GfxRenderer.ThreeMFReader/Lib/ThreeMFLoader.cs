using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Numerics;
using System.Xml.Linq;

namespace GfxRenderer.ThreeMFReader
{
    public static class ThreeMFLoader
    {
        public static List<ThreeMFMesh> LoadFromFile(string fileName)
        {
            using (var stream = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                using (var zip = new ZipArchive(stream))
                {
                    var model = zip.GetEntry("3D/3dmodel.model");
                    return LoatFromStream(model.Open()).ToList();
                }
            }
        }

        private static IEnumerable<ThreeMFMesh> LoatFromStream(Stream stream)
        {
            var xdocument = XDocument.Load(stream);
            var ns = xdocument.Root.GetDefaultNamespace();
            var cultureCode = xdocument.Root.Attribute(XNamespace.Xml.GetName("lang"))?.Value ?? "en-US";
            var meshes = xdocument.Root.Descendants(ns.GetName("mesh"));
            var ci = CultureInfo.GetCultureInfo(cultureCode);

            foreach (var mesh in meshes)
            {
                var vectors = mesh.Descendants(ns.GetName("vertex")).ToList();
                var triangles = mesh.Descendants(ns.GetName("triangle")).ToList();
                var threeMfMesh = new ThreeMFMesh();
                threeMfMesh.Vertices = vectors.Select(v =>
                {
                    var x = float.Parse(v.Attribute("x").Value, ci);
                    var y = float.Parse(v.Attribute("y").Value, ci);
                    var z = float.Parse(v.Attribute("z").Value, ci);
                    return new Vector3(x, y, z);
                }).ToArray();

                threeMfMesh.Triangles = triangles.Select(t =>
                {
                    var v1 = int.Parse(t.Attribute("v1").Value, ci);
                    var v2 = int.Parse(t.Attribute("v2").Value, ci);
                    var v3 = int.Parse(t.Attribute("v3").Value, ci);
                    return new Triangle { Vector1 = v1, Vector2 = v2, Vector3 = v3 };
                }).ToArray();

                threeMfMesh.Normals = new Vector3[triangles.Count];

                for (int i = 0; i < threeMfMesh.Triangles.Length; ++i)
                {
                    var v1 = threeMfMesh.Vertices[threeMfMesh.Triangles[i].Vector1];
                    var v2 = threeMfMesh.Vertices[threeMfMesh.Triangles[i].Vector2];
                    var v3 = threeMfMesh.Vertices[threeMfMesh.Triangles[i].Vector3];
                    threeMfMesh.Normals[i] = Vector3.Normalize(Vector3.Cross(v2 - v1, v3 - v1));
                }

                yield return threeMfMesh;
            }
        }
    }
}
