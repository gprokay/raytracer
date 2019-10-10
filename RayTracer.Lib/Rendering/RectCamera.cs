using System.Numerics;

namespace RayTracer.Lib
{
    public class RectCamera : ICamera
    {
        public class RayFactory : IRayFactory
        {
            private Vector3 horizontalUnitVector;
            private Vector3 verticalUnitVector;
            private Vector3 rayStartPoint;
            private Vector3 topLeft;

            public RayFactory(RectCamera camera, int pixelWidth, int pixelHeight)
            {
                var horizontalUnit = camera.xVector.Length() / pixelWidth;
                var verticalUnit = camera.yVector.Length() / pixelHeight;

                horizontalUnitVector = horizontalUnit * camera.planeXVector;
                verticalUnitVector = verticalUnit * camera.planeYVector;

                var halfVerticalLenght = (float)pixelHeight / 2;
                var halfHorizintalLength = (float)pixelWidth / 2;

                var center = camera.planeCenter;

                rayStartPoint = camera.focalPoint;

                topLeft = center - halfHorizintalLength * horizontalUnitVector - halfVerticalLenght * verticalUnitVector;
            }

            public Ray GetCameraRay(int x, int y)
            {
                var current = topLeft + x * horizontalUnitVector + y * verticalUnitVector;
                return new Ray(rayStartPoint, current - rayStartPoint);
            }
        }

        private readonly Vector3 xVector;
        private readonly Vector3 yVector;
        private readonly Vector3 planeCenter;
        private readonly Vector3 focalPoint;

        private readonly Vector3 planeXVector;
        private readonly Vector3 planeYVector;

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
        }

        public Ray[] GetCameraRays(int pixelWidth, int pixelHeight)
        {
            var rays = new Ray[pixelWidth * pixelHeight];

            var horizontalUnit = xVector.Length() / pixelWidth;
            var verticalUnit = yVector.Length() / pixelHeight;

            var horizontalUnitVector = horizontalUnit * planeXVector;
            var verticalUnitVector = verticalUnit * planeYVector;

            var halfVerticalLenght = (float)pixelHeight / 2;
            var halfHorizintalLength = (float)pixelWidth / 2;

            var center = planeCenter;

            var rayStartPoint = focalPoint;

            var topLeft = center - halfHorizintalLength * horizontalUnitVector - halfVerticalLenght * verticalUnitVector;

            for (var x = 0; x < pixelWidth; ++x)
            {
                for (var y = 0; y < pixelHeight; ++y)
                {
                    var index = y * pixelWidth + x;
                    var current = topLeft + x * horizontalUnitVector + y * verticalUnitVector;
                    rays[index] = new Ray(rayStartPoint, current - rayStartPoint);
                }
            }

            return rays;
        }

        public IRayFactory GetRayFactory(int pixelWidth, int pixelHeight)
        {
            return new RayFactory(this, pixelWidth, pixelHeight);
        }
    }
}
