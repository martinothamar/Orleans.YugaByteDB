using System;
using BenchmarkDotNet.Running;

namespace Orleans.YugaByteDB.Benchmarks
{
    class Program
    {
        static void Main(string[] args)
        {
            var summary = BenchmarkRunner.Run<ProtoVsJson>();
        }
    }
}
