using Microsoft.VisualStudio.TestTools.UnitTesting;
using Amicitia.IO.Binary;
using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Numerics;
using System.Runtime.InteropServices;
using Amicitia.IO.Streams;

namespace Amicitia.IO.Binary.Tests
{
    [TestClass()]
    public class BinaryValueWriterTests
    {
        private static Func<Stream, StreamOwnership, Endianness, BinaryValueWriter> writerFactory
            = ( stream, streamOwnership, endianness ) => new BinaryValueWriter( stream, streamOwnership, endianness );

        [TestMethod()]
        public void WriteBitTest() => WriteBitTest( writerFactory );

        [TestMethod()]
        public void WriteBitTestIndexed() => WriteBitTestIndexed( writerFactory );

        [TestMethod()]
        public void WriteTest()  => WriteTest( writerFactory );

        [TestMethod()]
        public void WriteArrayTest() => WriteArrayTest( writerFactory );

        [TestMethod()]
        public void WriteCollectionTest() => WriteCollectionTest( writerFactory );

        [TestMethod()]
        public void WriteStringTest() => WriteStringTest( writerFactory );

        public static void WriteBitTest(Func<Stream, StreamOwnership, Endianness, BinaryValueWriter> writerFactory)
        {
            var stream = new MemoryStream();
            using ( var writer = writerFactory( stream, StreamOwnership.Retain, Endianness.Little ) )
            {
                writer.WriteBit( true );
                writer.WriteBit( true );
                writer.WriteBit( false );
                writer.WriteBit( true );
                writer.Seek( 3, SeekOrigin.Current );
                writer.WriteBit( false );
                writer.WriteBit( false );
                writer.WriteBit( true );
                writer.WriteBit( false );
            }
            stream.Position = 0;

            using ( var reader = new BinaryValueReader( stream, StreamOwnership.Transfer, Endianness.Little, Encoding.Default ) )
            {
                Assert.IsTrue( reader.ReadBit() );
                Assert.IsTrue( reader.ReadBit() );
                Assert.IsFalse( reader.ReadBit() );
                Assert.IsTrue( reader.ReadBit() );
                reader.Seek( 3, SeekOrigin.Current );
                Assert.IsFalse( reader.ReadBit() );
                Assert.IsFalse( reader.ReadBit() );
                Assert.IsTrue( reader.ReadBit() );
                Assert.IsFalse( reader.ReadBit() );
            }
        }

        public static void WriteBitTestIndexed( Func<Stream, StreamOwnership, Endianness, BinaryValueWriter> writerFactory )
        {
            var stream = new MemoryStream();
            using ( var writer = writerFactory( stream, StreamOwnership.Retain, Endianness.Little ) )
            {
                writer.WriteBit( 0, true );
                writer.WriteBit( 1, true );
                writer.WriteBit( 2, false );
                writer.WriteBit( 3, true );
                writer.Seek( 3, SeekOrigin.Current );
                writer.WriteBit( 0, false );
                writer.WriteBit( 1, false );
                writer.WriteBit( 2, true );
                writer.WriteBit( 3, false );
            }
            stream.Position = 0;

            using ( var reader = new BinaryValueReader( stream, StreamOwnership.Retain, Endianness.Little, Encoding.Default ) )
            {
                Assert.IsTrue( reader.ReadBit() );
                Assert.IsTrue( reader.ReadBit() );
                Assert.IsFalse( reader.ReadBit() );
                Assert.IsTrue( reader.ReadBit() );
                reader.Seek( 3, SeekOrigin.Current );
                Assert.IsFalse( reader.ReadBit() );
                Assert.IsFalse( reader.ReadBit() );
                Assert.IsTrue( reader.ReadBit() );
                Assert.IsFalse( reader.ReadBit() );
            }
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        struct TestStruct
        {
            public int Field1;
            public uint Field2;
            public float Field3;
            public byte Field4;
        }

        public static void WriteTest( Func<Stream, StreamOwnership, Endianness, BinaryValueWriter> writerFactory )
        {
            var leStream = new MemoryStream();
            using ( var writer = writerFactory( leStream, StreamOwnership.Retain, Endianness.Little  ) )
            {
                writer.Write<int>( 0x12345678 );
                writer.Write<uint>( 0xDEADBEEF );
                writer.Write<float>( 1.0f );
                writer.Write<byte>( 0xFF );
            }

            leStream.Position = 0;

            var beStream = new MemoryStream();
            using ( var writer = writerFactory( beStream, StreamOwnership.Retain, Endianness.Big ) )
            {
                writer.Write<int>( 0x12345678 );
                writer.Write<uint>( 0xDEADBEEF );
                writer.Write<float>( 1.0f );
                writer.Write<byte>( 0xFF );
            }

            beStream.Position = 0;

            var structStream = new MemoryStream();
            using ( var writer = writerFactory( structStream, StreamOwnership.Retain, Endianness.Big ) )
            {
                writer.Write<TestStruct>( new TestStruct() { Field1 = 0x12345678, Field2 = 0xDEADBEEF, Field3 = 1.0f, Field4 = 0xFF } );
            }
            structStream.Position = 0;

            void DoTests( MemoryStream stream, BinaryValueReader reader )
            {
                var bytes = stream.ToArray();
                stream.Position = 0;

                for ( int i = 0; i < bytes.Length; i++ )
                    Assert.IsTrue( reader.Read<byte>() == bytes[i] );

                reader.Seek( 0, SeekOrigin.Begin );
                Assert.IsTrue( reader.Read<int>() == 0x12345678 );
                Assert.IsTrue( reader.Read<uint>() == 0xDEADBEEF );
                Assert.IsTrue( reader.Read<float>() == 1.0f );

                reader.Seek( 0, SeekOrigin.Begin );
                var testStruct = reader.Read<TestStruct>();
                Assert.IsTrue( testStruct.Field1 == 0x12345678 );
                Assert.IsTrue( testStruct.Field2 == 0xDEADBEEF );
                Assert.IsTrue( testStruct.Field3 == 1.0f );
                Assert.IsTrue( testStruct.Field4 == 0xFF );
            }

            // Default buffer size
            DoTests( leStream, new BinaryValueReader( leStream, StreamOwnership.Transfer, Endianness.Little ) );
            DoTests( beStream, new BinaryValueReader( beStream, StreamOwnership.Transfer, Endianness.Big ) );
            DoTests( structStream, new BinaryValueReader( beStream, StreamOwnership.Transfer, Endianness.Big ) );

            // No buffer
            DoTests( leStream, new BinaryValueReader( leStream, StreamOwnership.Transfer, Endianness.Little, Encoding.Default, null, 0 ) );
            DoTests( beStream, new BinaryValueReader( beStream, StreamOwnership.Transfer, Endianness.Big, Encoding.Default, null, 0 ) );
            DoTests( structStream, new BinaryValueReader( structStream, StreamOwnership.Transfer, Endianness.Big, Encoding.Default, null, 0 ) );

            // Buffer smaller than sample size
            DoTests( leStream, new BinaryValueReader( leStream, StreamOwnership.Transfer, Endianness.Little, Encoding.Default, null, 3 ) );
            DoTests( beStream, new BinaryValueReader( beStream, StreamOwnership.Transfer, Endianness.Big, Encoding.Default, null, 3 ) );
            DoTests( structStream, new BinaryValueReader( structStream, StreamOwnership.Transfer, Endianness.Big, Encoding.Default, null, 0 ) );
        }

        public static void WriteArrayTest( Func<Stream, StreamOwnership, Endianness, BinaryValueWriter> writerFactory )
        {
            void DoTests( byte[] bytes, BinaryValueReader reader )
            {
                var readBytes = reader.ReadArray<byte>( bytes.Length );
                CollectionAssert.AreEqual( bytes, readBytes );

                reader.Seek( 0, SeekOrigin.Begin );
                var readFloats = reader.ReadArray<float>(3);
                CollectionAssert.AreEqual( new[] { 1f, 2f, 3f }, readFloats );

                reader.Seek( 0, SeekOrigin.Begin );
                var readVecs = reader.ReadArray<Vector3>(1);
                Assert.IsTrue( readVecs.Length == 1 );
                Assert.IsTrue( readVecs[0].X == 1.0f );
                Assert.IsTrue( readVecs[0].Y == 2.0f );
                Assert.IsTrue( readVecs[0].Z == 3.0f );
            }

            byte[] leBytes, beBytes;
            var leStream = new MemoryStream();
            using ( var writer = writerFactory( leStream, StreamOwnership.Retain, Endianness.Little ) )
            {
                writer.WriteArray( new float[] { 1f, 2f, 3f } );
                writer.Write<uint>( 0xF0DADEFA );
            }
            leBytes = leStream.ToArray();

            var beStream = new MemoryStream();
            using ( var writer = writerFactory( beStream, StreamOwnership.Retain, Endianness.Big ) )
            {
                writer.WriteArray( new float[] { 1f, 2f, 3f } );
                writer.Write<uint>( 0xF0DADEFA );
            }
            beBytes = beStream.ToArray();

            DoTests( leBytes, new BinaryValueReader( new MemoryStream( leBytes ), StreamOwnership.Transfer, Endianness.Little, Encoding.Default ) );
            DoTests( beBytes, new BinaryValueReader( new MemoryStream( beBytes ), StreamOwnership.Transfer, Endianness.Big, Encoding.Default ) );
        }

        public static void WriteCollectionTest( Func<Stream, StreamOwnership, Endianness, BinaryValueWriter> writerFactory )
        {
            void DoTests( byte[] bytes, BinaryValueReader reader )
            {
                var readBytes = reader.ReadArray<byte>( bytes.Length );
                CollectionAssert.AreEqual( bytes, readBytes );

                reader.Seek( 0, SeekOrigin.Begin );
                var readFloats = reader.ReadArray<float>(3);
                CollectionAssert.AreEqual( new[] { 1f, 2f, 3f }, readFloats );

                reader.Seek( 0, SeekOrigin.Begin );
                var readVecs = reader.ReadArray<Vector3>(1);
                Assert.IsTrue( readVecs.Length == 1 );
                Assert.IsTrue( readVecs[0].X == 1.0f );
                Assert.IsTrue( readVecs[0].Y == 2.0f );
                Assert.IsTrue( readVecs[0].Z == 3.0f );
            }

            byte[] leBytes, beBytes;
            var leStream = new MemoryStream();
            using ( var writer = writerFactory( leStream, StreamOwnership.Retain, Endianness.Little ) )
            {
                writer.WriteCollection( new float[] { 1f, 2f, 3f } );
                writer.Write<uint>( 0xF0DADEFA );
            }
            leBytes = leStream.ToArray();

            var beStream = new MemoryStream();
            using ( var writer = writerFactory( beStream, StreamOwnership.Retain, Endianness.Big ) )
            {
                writer.WriteCollection( new float[] { 1f, 2f, 3f } );
                writer.Write<uint>( 0xF0DADEFA );
            }
            beBytes = beStream.ToArray();

            DoTests( leBytes, new BinaryValueReader( new MemoryStream( leBytes ), StreamOwnership.Transfer, Endianness.Little, Encoding.Default ) );
            DoTests( beBytes, new BinaryValueReader( new MemoryStream( beBytes ), StreamOwnership.Transfer, Endianness.Big, Encoding.Default ) );
        }

        public static void WriteStringTest( Func<Stream, StreamOwnership, Endianness, BinaryValueWriter> writerFactory )
        {
            byte[] GetBytes(StringBinaryFormat format, string value, int fixedLength)
            {
                var stream = new MemoryStream();
                using ( var writer = writerFactory( stream, StreamOwnership.Retain, Endianness.Little ) )
                    writer.WriteString( format, value, fixedLength );
                return stream.ToArray();
            }

            var nullTerminatedStringBytes = GetBytes(StringBinaryFormat.NullTerminated, "HELLO\0DEAD", -1);
            var fixedLengthStringBytes = GetBytes(StringBinaryFormat.FixedLength, "HELLO\0DEAD", 5);
            var prefix8StringBytes = GetBytes(StringBinaryFormat.PrefixedLength8, "TEST", -1);
            var prefix16StringBytes = GetBytes(StringBinaryFormat.PrefixedLength16, "TEST", -1);
            var prefix32StringBytes = GetBytes(StringBinaryFormat.PrefixedLength32, "TEST", -1);
            var prefix64StringBytes = GetBytes(StringBinaryFormat.PrefixedLength64, "TEST", -1);

            void DoTest( byte[] bytes, StringBinaryFormat format, int fixedLength, string expected )
            {
                using ( var reader = new BinaryValueReader( new MemoryStream( bytes ), StreamOwnership.Transfer, Endianness.Little, Encoding.Default ) )
                {
                    var value = reader.ReadString(format, fixedLength);
                    Assert.AreEqual( expected, value );
                }
            }

            DoTest( nullTerminatedStringBytes, StringBinaryFormat.NullTerminated, -1, "HELLO" );
            DoTest( fixedLengthStringBytes, StringBinaryFormat.FixedLength, fixedLengthStringBytes.Length, "HELLO" );
            DoTest( prefix8StringBytes, StringBinaryFormat.PrefixedLength8, -1, "TEST" );
            DoTest( prefix16StringBytes, StringBinaryFormat.PrefixedLength16, -1, "TEST" );
            DoTest( prefix32StringBytes, StringBinaryFormat.PrefixedLength32, -1, "TEST" );
            DoTest( prefix64StringBytes, StringBinaryFormat.PrefixedLength64, -1, "TEST" );
        }
    }
}