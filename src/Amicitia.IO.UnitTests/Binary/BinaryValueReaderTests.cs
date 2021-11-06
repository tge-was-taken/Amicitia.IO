using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Text;
using System.IO;
using System.Numerics;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Amicitia.IO.Streams;

namespace Amicitia.IO.Binary.Tests
{
    [TestClass()]
    public class BinaryValueReaderTests
    {
        [TestMethod()]
        public void ReadBitTest()
        {
            var bytes = new byte[] { 0b00001011, 0x00, 0x00, 0x00, 0b00000100 };
            var reader = new BinaryValueReader( new MemoryStream(bytes), StreamOwnership.Transfer, Endianness.Little, Encoding.Default );
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

        [TestMethod()]
        public void ReadBitTestIndexed()
        {
            var bytes = new byte[] { 0b00001011, 0x00, 0x00, 0x00, 0b00000100 };
            var reader = new BinaryValueReader( new MemoryStream(bytes), StreamOwnership.Transfer, Endianness.Little, Encoding.Default );
            Assert.IsTrue( reader.ReadBit(0) );
            Assert.IsTrue( reader.ReadBit(1) );
            Assert.IsFalse( reader.ReadBit(2) );
            Assert.IsTrue( reader.ReadBit(3) );
            reader.Seek( 3, SeekOrigin.Current );
            Assert.IsFalse( reader.ReadBit(0) );
            Assert.IsFalse( reader.ReadBit(1) );
            Assert.IsTrue( reader.ReadBit(2) );
            Assert.IsFalse( reader.ReadBit(3) );
        }
        
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        struct TestStruct
        {
            public TestEnum Field1;
            public int Field2;
            public uint Field3;     
            public float Field4;
            public byte Field5;
        }

        enum TestEnum : short
        {
            Zero, One, Two
        }

        [TestMethod()]
        public void ReadTest()
        {
            var leBytes = new byte[] { 0x01, 0x00, 0x78, 0x56, 0x34, 0x12, 0xEF, 0xBE, 0xAD, 0xDE, 0x00, 0x00, 0x80, 0x3F, 0xFF };
            var beBytes = new byte[] { 0x00, 0x01, 0x12, 0x34, 0x56, 0x78, 0xDE, 0xAD, 0xBE, 0xEF, 0x3F, 0x80, 0x00, 0x00, 0xFF };

            void DoTests( byte[] bytes, BinaryValueReader reader )
            {
                for ( int i = 0; i < bytes.Length; i++ )
                    Assert.IsTrue( reader.Read<byte>() == bytes[i] );

                reader.Seek( 0, SeekOrigin.Begin );
                Assert.IsTrue( reader.Read<TestEnum>() == TestEnum.One );
                Assert.IsTrue( reader.Read<int>() == 0x12345678 );
                Assert.IsTrue( reader.Read<uint>() == 0xDEADBEEF );
                Assert.IsTrue( reader.Read<float>() == 1.0f );

                reader.Seek( 0, SeekOrigin.Begin );
                var testStruct = reader.Read<TestStruct>();
                Assert.IsTrue( testStruct.Field1 == TestEnum.One );
                Assert.IsTrue( testStruct.Field2 == 0x12345678 );
                Assert.IsTrue( testStruct.Field3 == 0xDEADBEEF );
                Assert.IsTrue( testStruct.Field4 == 1.0f );
                Assert.IsTrue( testStruct.Field5 == 0xFF );
            }

            // Default buffer size
            DoTests( leBytes, new BinaryValueReader( new MemoryStream( leBytes ), StreamOwnership.Transfer, Endianness.Little, Encoding.Default ) );
            DoTests( beBytes, new BinaryValueReader( new MemoryStream( beBytes ), StreamOwnership.Transfer, Endianness.Big, Encoding.Default ) );

            // No buffer
            DoTests( leBytes, new BinaryValueReader( new MemoryStream( leBytes ), StreamOwnership.Transfer, Endianness.Little, Encoding.Default, null, 0 ) );
            DoTests( beBytes, new BinaryValueReader( new MemoryStream( beBytes ), StreamOwnership.Transfer, Endianness.Big, Encoding.Default, null, 0 ) );

            // Buffer smaller than sample size
            DoTests( leBytes, new BinaryValueReader( new MemoryStream( leBytes ), StreamOwnership.Transfer, Endianness.Little, Encoding.Default, null, 3 ) );
            DoTests( beBytes, new BinaryValueReader( new MemoryStream( beBytes ), StreamOwnership.Transfer, Endianness.Big, Encoding.Default, null, 3 ) );
        }

        [TestMethod()]
        public void ReadArrayTest()
        {
            void DoTests(byte[] bytes, BinaryValueReader reader)
            {
                var readBytes = reader.ReadArray<byte>( bytes.Length );
                CollectionAssert.AreEqual( bytes, readBytes );

                reader.Seek( 0, SeekOrigin.Begin );
                var readFloats = reader.ReadArray<float>(3);
                CollectionAssert.AreEqual( new[] { 1f, 1f, 1f }, readFloats );

                reader.Seek( 0, SeekOrigin.Begin );
                var readVecs = reader.ReadArray<Vector3>(1);
                Assert.IsTrue( readVecs.Length == 1 );
                Assert.IsTrue( readVecs[0].X == 1.0f );
                Assert.IsTrue( readVecs[0].Y == 1.0f );
                Assert.IsTrue( readVecs[0].Z == 1.0f );
            }

            var leBytes = new byte[]{ 0x00, 0x00, 0x80, 0x3F, 0x00, 0x00, 0x80, 0x3F, 0x00, 0x00, 0x80, 0x3F, 0xFA, 0xDE, 0xDA, 0xF0 };
            var beBytes = new byte[]{ 0x3F, 0x80, 0x00, 0x00, 0x3F, 0x80, 0x00, 0x00, 0x3F, 0x80, 0x00, 0x00, 0xF0, 0xDA, 0xDE, 0xFA };

            DoTests( leBytes, new BinaryValueReader( new MemoryStream( leBytes ), StreamOwnership.Transfer, Endianness.Little, Encoding.Default ) );
            DoTests( beBytes, new BinaryValueReader( new MemoryStream( beBytes ), StreamOwnership.Transfer, Endianness.Big, Encoding.Default ) );
        }

        [TestMethod()]
        public void ReadCollectionTest()
        {
            void DoTests( byte[] bytes, BinaryValueReader reader )
            {
                var readBytes = new List<byte>(bytes.Length);
                reader.ReadCollection( bytes.Length, readBytes );
                CollectionAssert.AreEqual( bytes, readBytes );

                reader.Seek( 0, SeekOrigin.Begin );
                var readFloats = new List<float>();
                reader.ReadCollection( 3, readFloats );
                CollectionAssert.AreEqual( new[] { 1f, 1f, 1f }, readFloats );

                reader.Seek( 0, SeekOrigin.Begin );
                var readVecs = new List<Vector3>(1);
                reader.ReadCollection( 1, readVecs );
                Assert.IsTrue( readVecs.Count == 1 );
                Assert.IsTrue( readVecs[0].X == 1.0f );
                Assert.IsTrue( readVecs[0].Y == 1.0f );
                Assert.IsTrue( readVecs[0].Z == 1.0f );
            }

            var leBytes = new byte[]{ 0x00, 0x00, 0x80, 0x3F, 0x00, 0x00, 0x80, 0x3F, 0x00, 0x00, 0x80, 0x3F, 0xFA, 0xDE, 0xDA, 0xF0 };
            var beBytes = new byte[]{ 0x3F, 0x80, 0x00, 0x00, 0x3F, 0x80, 0x00, 0x00, 0x3F, 0x80, 0x00, 0x00, 0xF0, 0xDA, 0xDE, 0xFA };

            DoTests( leBytes, new BinaryValueReader( new MemoryStream( leBytes ), StreamOwnership.Transfer, Endianness.Little, Encoding.Default ) );
            DoTests( beBytes, new BinaryValueReader( new MemoryStream( beBytes ), StreamOwnership.Transfer, Endianness.Big, Encoding.Default ) );
        }

        [TestMethod()]
        public void ReadStringTest()
        {
            var nullTerminatedStringBytes = new byte[] { (byte)'H', ( byte )'E', ( byte )'L', ( byte )'L', ( byte )'O', 0, ( byte )'D', ( byte )'E', ( byte )'A', ( byte )'D', 0};
            var prefix8StringBytes = new byte[] { 0x04, (byte)'T', ( byte )'E', ( byte )'S', ( byte )'T', ( byte )'B', ( byte )'A', ( byte )'D' };
            var prefix16StringBytes = new byte[] { 0x04, 0x00, (byte)'T', ( byte )'E', ( byte )'S', ( byte )'T', ( byte )'B', ( byte )'A', ( byte )'D' };
            var prefix32StringBytes = new byte[] { 0x04, 0x00, 0x00, 0x00, (byte)'T', ( byte )'E', ( byte )'S', ( byte )'T', ( byte )'B', ( byte )'A', ( byte )'D' };
            var prefix64StringBytes = new byte[] { 0x04, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, ( byte)'T', ( byte )'E', ( byte )'S', ( byte )'T', ( byte )'B', ( byte )'A', ( byte )'D' };

            void DoTest(byte[] bytes, StringBinaryFormat format, int fixedLength, string expected)
            {
                using ( var reader = new BinaryValueReader( new MemoryStream( bytes ), StreamOwnership.Transfer, Endianness.Little, Encoding.Default ) )
                {
                    var value = reader.ReadString(format, fixedLength);
                    Assert.AreEqual( expected, value );
                }
            }

            DoTest( nullTerminatedStringBytes, StringBinaryFormat.NullTerminated, -1, "HELLO" );
            DoTest( nullTerminatedStringBytes, StringBinaryFormat.FixedLength, nullTerminatedStringBytes.Length, "HELLO" );
            DoTest( prefix8StringBytes, StringBinaryFormat.PrefixedLength8, -1, "TEST" );
            DoTest( prefix16StringBytes, StringBinaryFormat.PrefixedLength16, -1, "TEST" );
            DoTest( prefix32StringBytes, StringBinaryFormat.PrefixedLength32, -1, "TEST" );
            DoTest( prefix64StringBytes, StringBinaryFormat.PrefixedLength64, -1, "TEST" );
        }
    }
}