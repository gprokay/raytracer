using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using RayTracer.Test;

namespace RayTracer.Bench
{
    [MemoryDiagnoser]
    public class RaytracerBench
    {

        //[Params(100, 200, 300, 400, 500, 600, 700, 800, 900, 1000, 1100, 1200, 1300, 1400, 1500, 1600, 1700, 1800, 1900, 2000, 3000, 4000, 5000, 6000, 7000, 8000, 9000, 10000)]
        //[Params(100, 200, 500, 1000, 2000, 5000)]
        //[Params(5000)]
        [Params(100, 200, 500, 1000)]
        public int size = 1000;

        [Benchmark]
        public void Parallel()
        {
            Run(true);
        }

        [Benchmark]
        public void Test()
        {
            Run(false);
        }

        private void Run(bool parallel)
        {
            var width = size;
            var height = (int)(width * (9f / 16f));

            SceneTest.RenderScene(width, height, parallel);
        }
    }


    class Program
    {
        static void Main(string[] args)
        {
            var summary = BenchmarkRunner.Run<RaytracerBench>();
        }
    }
}

