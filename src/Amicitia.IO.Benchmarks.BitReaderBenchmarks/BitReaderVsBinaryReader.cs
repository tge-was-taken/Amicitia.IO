using System.Diagnostics;
using System.IO;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using Amicitia.IO.Binary;
using BenchmarkDotNet.Attributes;

namespace Amicitia.IO.Benchmarks
{
    public class BitReaderVsBinaryReader
    {
        [StructLayout(LayoutKind.Sequential, Pack = 1, Size = 0x38)]
        public struct TestData
        {
            public int Field00;
            public float Field04;
            public short Field06;
            public ushort Field08;
            public double Field0C;
            public Vector3 Field14;
            public Vector3 Field20;
            public Vector3 Field2C;
        }

        private MemoryStream mMemoryStream1;
        private MemoryStream mMemoryStream2;

        private BinaryReader mBinaryReader;
        private BinaryValueReader mBitReader;
        private BinaryValueReader mBitReaderBE;

        public int TestDataCount { get; set; } = 1000000;

        [GlobalSetup]
        public void GlobalSetup()
        {
            var size = Unsafe.SizeOf<TestData>();
            Trace.Assert( size == 0x38 );
            mMemoryStream1 = new MemoryStream( new byte[size * TestDataCount] );
            mMemoryStream2 = new MemoryStream( new byte[size * TestDataCount] );
        }

        [IterationSetup]
        public void IterationSetup()
        {
            mMemoryStream1.Position = 0;
            mMemoryStream2.Position = 0;

            mBinaryReader = new BinaryReader( mMemoryStream1 );
            mBitReader = new BinaryValueReader( mMemoryStream2, StreamOwnership.Retain, Endianness.Little );
            mBitReaderBE = new BinaryValueReader( mMemoryStream2, StreamOwnership.Retain, Endianness.Big );
        }

        [IterationCleanup]
        public void IterationCleanup()
        {
        }

        [Benchmark]
        public TestData[] BinaryReader()
        {
            var testData = new TestData[TestDataCount];
            for ( int i = 0; i < testData.Length; i++ )
            {
                TestData data;
                data.Field00 = mBinaryReader.ReadInt32();
                data.Field04 = mBinaryReader.ReadSingle();
                data.Field06 = mBinaryReader.ReadInt16();
                data.Field08 = mBinaryReader.ReadUInt16();
                data.Field0C = mBinaryReader.ReadDouble();
                data.Field14.X = mBinaryReader.ReadSingle();
                data.Field14.Y = mBinaryReader.ReadSingle();
                data.Field14.Z = mBinaryReader.ReadSingle();
                data.Field20.X = mBinaryReader.ReadSingle();
                data.Field20.Y = mBinaryReader.ReadSingle();
                data.Field20.Z = mBinaryReader.ReadSingle();
                data.Field2C.X = mBinaryReader.ReadSingle();
                data.Field2C.Y = mBinaryReader.ReadSingle();
                data.Field2C.Z = mBinaryReader.ReadSingle();
                testData[i] = data;
            }

            return testData;
        }

        [Benchmark]
        public TestData[] BitReader() => mBitReader.ReadArray<TestData>( TestDataCount );

        [Benchmark]
        public TestData[] BitReaderBE() => mBitReaderBE.ReadArray<TestData>( TestDataCount );

        [Benchmark]
        public TestData[] BitReaderBEManualSwap()
        {
            var testData = mBitReader.ReadArray<TestData>(TestDataCount);
            for ( int i = 0; i < testData.Length; i++ )
            {
                BinaryOperations<int>.Reverse( ref testData[i].Field00 );
                BinaryOperations<float>.Reverse( ref testData[i].Field04 );
                BinaryOperations<short>.Reverse( ref testData[i].Field06 );
                BinaryOperations<ushort>.Reverse( ref testData[i].Field08 );
                BinaryOperations<double>.Reverse( ref testData[i].Field0C );
                BinaryOperations<float>.Reverse( ref testData[i].Field14.X );
                BinaryOperations<float>.Reverse( ref testData[i].Field14.Y );
                BinaryOperations<float>.Reverse( ref testData[i].Field14.Z );
                BinaryOperations<float>.Reverse( ref testData[i].Field20.X );
                BinaryOperations<float>.Reverse( ref testData[i].Field20.Y );
                BinaryOperations<float>.Reverse( ref testData[i].Field20.Z );
                BinaryOperations<float>.Reverse( ref testData[i].Field2C.X );
                BinaryOperations<float>.Reverse( ref testData[i].Field2C.Y );
                BinaryOperations<float>.Reverse( ref testData[i].Field2C.Z );
            }

            return testData;
        }
    }
}
