using System;
using System.Drawing;

namespace GfxRenderer.Lib
{
    public struct TraceContext
    {
        public Func<Ray, int, Color> TraceFunc;
        public Intersection Intersection;
        public Ray Ray;
        public bool IsSurface;
        public int Depth;
    }
}
