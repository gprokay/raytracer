using System;
using System.Drawing;

namespace RayTracer.Lib
{
    public struct TraceContext
    {
        public Func<Ray, int, Color> TraceFunc { get; set; }

        public Intersection Intersection { get; set; }

        public Ray Ray { get; set; }

        public bool IsSurface { get; set; }

        public int Depth { get; set; }
    }

}
