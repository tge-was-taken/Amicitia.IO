using System;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Running;

namespace Amicitia.IO.Benchmarks.BitReaderBenchmarks
{
    class Program
    {
        static void Main( string[] args )
        {
#if DEBUG
            IConfig config = new DebugInProcessConfig();
#else
            IConfig config = DefaultConfig.Instance;
#endif
            var summary = BenchmarkRunner.Run<BitReaderVsBinaryReader>(config);
        }
    }
}
