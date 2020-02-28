using System.Security.Cryptography;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Running;

namespace Amicitia.IO.Benchmarks
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
            //var summary = BenchmarkRunner.Run<FileStreamVsCachedBlockBufferedStream>(config);
            var summary = BenchmarkRunner.Run<BitReaderVsBinaryReader>(config);
        }
    }
}
