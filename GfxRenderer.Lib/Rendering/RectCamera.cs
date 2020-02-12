using System;
using System.Drawing;
using System.Numerics;

namespace GfxRenderer.Lib
{
    public class RectCamera : ICamera
    {
        public class RayFactory : IRayFactory
        {
            private Vector3 horizontalUnitVector;
            private Vector3 verticalUnitVector;
            private Vector3 rayStartPoint;
            private Vector3 topLeft;
            private Vector3 bottomRight;
            private Vector2 topLeftPoint;
            private Vector2 bottomRightPoint;
            private RectCamera camera;
            private readonly int pixelWidth;
            private readonly int pixelHeight;

            public int Width => pixelWidth;
            public int Height => pixelHeight;

            public RayFactory(RectCamera camera, int pixelWidth, int pixelHeight)
            {
                this.camera = camera;
                this.pixelWidth = pixelWidth;
                this.pixelHeight = pixelHeight;
                var horizontalUnit = camera.xVector.Length() / pixelWidth;
                var verticalUnit = camera.yVector.Length() / pixelHeight;

                horizontalUnitVector = horizontalUnit * camera.planeXVector;
                verticalUnitVector = verticalUnit * camera.planeYVector;

                var halfVerticalLenght = (float)pixelHeight / 2;
                var halfHorizintalLength = (float)pixelWidth / 2;

                var center = camera.planeCenter;

                rayStartPoint = camera.focalPoint;

                topLeft = center - halfHorizintalLength * horizontalUnitVector - halfVerticalLenght * verticalUnitVector;
                bottomRight = center + halfHorizintalLength * horizontalUnitVector + halfVerticalLenght * verticalUnitVector;
                topLeftPoint = camera.rectangle3D.Get2DVectors(topLeft);
                bottomRightPoint = camera.rectangle3D.Get2DVectors(bottomRight);
            }

            public Ray GetCameraRay(int x, int y)
            {
                var current = topLeft + x * horizontalUnitVector + y * verticalUnitVector;
                return new Ray(rayStartPoint, current - rayStartPoint);
            }

            public Point GetPlaneIntersectionPoint(Vector3 v)
            {
                var ray = new Ray(v, camera.focalPoint - v);
                camera.rectangle3D.TryGet2DVectors(ray, out var current);

                var point = new Point();

                if (current.X < topLeftPoint.X)
                {
                    point.X = 0;
                }
                else if (current.X > bottomRightPoint.X)
                {
                    point.X = pixelWidth - 1;
                }
                else
                {
                    var c = current.X - topLeftPoint.X;
                    var t = bottomRightPoint.X - topLeftPoint.X;
                    point.X = (int)(c * pixelWidth / t);
                }

                if (current.Y < topLeftPoint.Y)
                {
                    point.Y = 0;
                }
                else if (current.Y > bottomRightPoint.Y)
                {
                    point.Y = pixelHeight - 1;
                }
                else
                {
                    var c = current.Y - topLeftPoint.Y;
                    var t = bottomRightPoint.Y - topLeftPoint.Y;
                    point.Y = (int)(c * pixelHeight / t);
                }

                return point;
            }
        }

        private readonly Vector3 xVector;
        private readonly Vector3 yVector;
        private readonly Vector3 planeCenter;
        private readonly Vector3 focalPoint;

        private readonly Vector3 planeXVector;
        private readonly Vector3 planeYVector;
        private readonly Rectangle3D rectangle3D;

        public RectCamera(Vector3 o, Vector3 xVector, Vector3 yVector, float length)
        {
            this.xVector = xVector;
            this.yVector = yVector;
            var d = yVector - xVector;
            planeCenter = o + xVector + d / 2;
            var normal = Vector3.Normalize(Vector3.Cross(xVector, yVector));
            focalPoint = planeCenter + normal * length;
            planeXVector = Vector3.Normalize(xVector);
            planeYVector = Vector3.Normalize(yVector);
            rectangle3D = new Rectangle3D(o, xVector, yVector);
        }

        public IRayFactory GetRayFactory(int pixelWidth, int pixelHeight)
        {
            return new RayFactory(this, pixelWidth, pixelHeight);
        }

        public void ProjectToZBuffer(Vector3 v1, Vector3 v2, Vector3 v3, Vector3 n, ZBufferItem[] zbuffer, MeshObject obj, int triangleIndex, IRayFactory rayFactory)
        {
            var p1 = rayFactory.GetPlaneIntersectionPoint(v1);
            var p2 = rayFactory.GetPlaneIntersectionPoint(v2);
            var p3 = rayFactory.GetPlaneIntersectionPoint(v3);

            //if (p1.X == 0 && p2.X == 0 && p3.X == 0
            //    || p1.Y == 0 && p2.Y == 0 && p3.Y == 0
            //    || p1.X == rayFactory.Width - 1 && p2.X == rayFactory.Width - 1 && p3.X == rayFactory.Width - 1
            //    || p1.Y == rayFactory.Height - 1 && p2.Y == rayFactory.Height - 1 && p3.Y == rayFactory.Height - 1)
            //{
            //    return;
            //}

            var minX = Math.Min(Math.Min(p1.X, p2.X), p3.X);
            var maxX = Math.Max(Math.Max(p1.X, p2.X), p3.X);

            var minY = Math.Min(Math.Min(p1.Y, p2.Y), p3.Y);
            var maxY = Math.Max(Math.Max(p1.Y, p2.Y), p3.Y);

            for (var x = minX; x <= maxX; ++x)
            {
                for (var y = minY; y <= maxY; ++y)
                {
                    var ray = rayFactory.GetCameraRay(x, y);
                    var isSurface = ray.IsSurfaceHit(n);
                    if (!isSurface)
                    {
                        return;
                    }

                    if (ray.TryIntersectTriangle(v1, v2, v3, out _, out _, out _))
                    {
                        var d = Vector3.Dot(v1 - ray.StartPoint, n) / Vector3.Dot(n, ray.Direction);
                        var i = y * rayFactory.Width + x;
                        if (zbuffer[i].Distance > d)
                        {
                            zbuffer[i].Distance = d;
                            zbuffer[i].Object = obj;
                            zbuffer[i].TriangleIndex = triangleIndex;
                            zbuffer[i].Normal = n;
                            zbuffer[i].Intersection = d * ray.Direction + ray.StartPoint;
                        }
                    }
                }
            }
        }
    }
}
