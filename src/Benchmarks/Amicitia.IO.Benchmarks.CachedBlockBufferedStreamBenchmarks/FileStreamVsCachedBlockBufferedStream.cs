using System;
using System.Collections.Generic;
using System.IO;
using Amicitia.IO.Streams;
using BenchmarkDotNet.Attributes;

namespace Amicitia.IO.Benchmarks.CachedBlockBufferedStreamBenchmarks
{
    public class FileStreamVsCachedBlockBufferedStream : IDisposable
    {
        private const int READ_OPS = 1000;
        private const int MAX_READ_SIZE = 8;
        private const int FILESIZE = 1024 * 1024 * 100;

        private FileStream mFileStream;
        private CachedBlockBufferedStream mCachedBlockBufferedStream;
        private MemoryStream mMemoryStream;
        private List<(int, int)> mReadOps;

        //[Params(1024 * 4, 1024 * 8, 1024 * 16, 1024 * 32, 1024 * 64, 1024 * 128, 1024 * 256, 1024 * 512, 1024 * 1024, 1024 * 1024 * 10)]
        public int BlockSize { get; set; } = 4096;

        //[Params(100, 1000, 10000, int.MaxValue)]
        public int BlockCount { get; set; } = int.MaxValue;

        [GlobalSetup]
        public void GlobalSetup()
        {
            var random = new Random();

            if ( !File.Exists( "FileStreamVsCachedBlockBufferedStream.mFileStream.bin" ) )
            {
                var data = new byte[FILESIZE];
                for ( int i = 0; i < data.Length; i++ )
                    data[i] = ( byte )random.Next( int.MinValue, int.MaxValue );

                File.WriteAllBytes( "FileStreamVsCachedBlockBufferedStream.mFileStream.bin", data );
                File.WriteAllBytes( "FileStreamVsCachedBlockBufferedStream.mCachedBlockBufferedStream.bin", data );
                File.WriteAllBytes( "FileStreamVsCachedBlockBufferedStream.mMemoryStream.bin", data );
            }

            mReadOps = new List<(int, int)>();
            for ( int i = 0; i < READ_OPS; i++ )
            {
                var offset = random.Next( 0, FILESIZE );
                mReadOps.Add( (offset, Math.Min( FILESIZE - offset, random.Next( 1, MAX_READ_SIZE + 1 ) )) );
            }
        }

        [IterationSetup]
        public void IterationSetup()
        {
            mFileStream = File.OpenRead( "FileStreamVsCachedBlockBufferedStream.mFileStream.bin" );
            mCachedBlockBufferedStream = new CachedBlockBufferedStream( File.OpenRead( "FileStreamVsCachedBlockBufferedStream.mCachedBlockBufferedStream.bin" ),
                BlockSize, BlockCount );

            mMemoryStream = new MemoryStream();
            using ( var fileStream = File.OpenRead( "FileStreamVsCachedBlockBufferedStream.mMemoryStream.bin" ) )
            {
                fileStream.CopyTo( mMemoryStream );
                mMemoryStream.Position = 0;
            }

        }

        [IterationCleanup]
        public void IterationCleanup()
        {
            mFileStream.Dispose();
            mCachedBlockBufferedStream.Dispose();
        }

        private byte[] PerformReadOps( Stream stream )
        {
            var readBuffer = new byte[MAX_READ_SIZE];

            for ( int i = 0; i < mReadOps.Count; i++ )
            {
                stream.Seek( mReadOps[i].Item1, SeekOrigin.Begin );
                stream.Read( readBuffer, 0, mReadOps[i].Item2 );
            }

            return readBuffer;
        }

        [Benchmark] public byte[] FileStream() => PerformReadOps( mFileStream );
        [Benchmark] public byte[] CachedBlockBufferedStream() => PerformReadOps( mCachedBlockBufferedStream );
        [Benchmark] public byte[] MemoryStream() => PerformReadOps( mMemoryStream );

        public void Dispose()
        {
            mFileStream.Dispose();
            mCachedBlockBufferedStream.Dispose();
        }
    }
}
