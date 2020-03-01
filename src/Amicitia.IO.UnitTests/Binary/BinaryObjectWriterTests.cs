using Microsoft.VisualStudio.TestTools.UnitTesting;
using Amicitia.IO.Binary;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Numerics;
using System.Text;
using System.Linq;

using Amicitia.IO.Streams;

namespace Amicitia.IO.Binary.Tests
{
    [TestClass()]
    public class BinaryObjectWriterTests
    {
        private static Func<Stream, StreamOwnership, Endianness, BinaryValueWriter> writerFactory
            = ( stream, streamOwnership, endianness ) => new BinaryObjectWriter( stream, streamOwnership, endianness );

        [TestMethod()]
        public void WriteBitTest() => BinaryValueWriterTests.WriteBitTest( writerFactory );

        [TestMethod()]
        public void WriteBitTestIndexed() => BinaryValueWriterTests.WriteBitTestIndexed( writerFactory );

        [TestMethod()]
        public void WriteTest() => BinaryValueWriterTests.WriteTest( writerFactory );

        [TestMethod()]
        public void WriteArrayTest() => BinaryValueWriterTests.WriteArrayTest( writerFactory );

        [TestMethod()]
        public void WriteCollectionTest() => BinaryValueWriterTests.WriteCollectionTest( writerFactory );

        [TestMethod()]
        public void WriteStringTest() => BinaryValueWriterTests.WriteStringTest( writerFactory );

        public class TestObject1 : IBinarySerializableWithInfo
        {
            public BinarySourceInfo BinarySourceInfo { get; set; }

            public int Field00 { get; set; }
            public Vector2 Field08 { get; set; }
            public TestObject1 Next { get; set; }

            public void Read( BinaryObjectReader reader )
            {
                Field00 = reader.Read<int>();
                Field08 = reader.Read<Vector2>();
                Next = reader.ReadObjectOffset<TestObject1>();
            }

            public void Write( BinaryObjectWriter writer )
            {
                writer.Write<int>( Field00 );
                writer.Write<Vector2>( Field08 );
                writer.WriteObjectOffset( Next );
            }
        }

        [TestMethod()]
        public void WriteObjectOffsetTest()
        {
            var rootObj = new TestObject1()
            {
                Field00 = 420,
                Field08 = new Vector2(420.69f, 69.420f),
            };

            rootObj.Next = new TestObject1()
            {
                Field00 = 69,
                Field08 = new Vector2( 69.420f, 420.69f ),
                Next    = rootObj,
            };

            var stream = new MemoryStream();
            using ( var writer = new BinaryObjectWriter( stream, StreamOwnership.Retain, Endianness.Little ) )
            {
                writer.WriteObjectOffset( rootObj );
                writer.WriteObjectOffset( rootObj.Next );
            }
             
            //File.WriteAllBytes( "temp.bin", stream.GetBuffer() );

            stream.Position = 0;

            using ( var reader = new BinaryObjectReader( stream, StreamOwnership.Retain, Endianness.Little ) )
            {
                var newRootObj = reader.ReadObjectOffset<TestObject1>();
                var newRootObjNext = reader.ReadObjectOffset<TestObject1>();

                // Assert values
                Debug.Assert( newRootObj.Field00 == 420 );
                Debug.Assert( newRootObj.Field08 == rootObj.Field08 );
                Debug.Assert( newRootObj.Next.Field00 == rootObj.Next.Field00 );
                Debug.Assert( newRootObj.Next.Field08 == rootObj.Next.Field08 );

                // Assert references
                Debug.Assert( newRootObjNext == newRootObj.Next );
                Debug.Assert( newRootObjNext.Next == newRootObj );

                // Assert priority
                //Debug.Assert( newRootObj.BinarySourceInfo.StartOffset > newRootObjNext.BinarySourceInfo.StartOffset,
                //              "First object should occur in the file after the second, because the second has a higher priority value" );
            }
        }

        [TestMethod()]
        public void WriteValueOffsetTest()
        {
            var stream = new MemoryStream();
            using ( var writer = new BinaryObjectWriter( stream, StreamOwnership.Retain, Endianness.Little ) )
            {
                writer.WriteValueOffset( 420 );
                writer.WriteValueOffset( 69, alignment: 16 );
            }

            stream.Position = 0;
            using ( var reader = new BinaryObjectReader( stream, StreamOwnership.Retain, Endianness.Little ) )
            {
                Assert.AreEqual( 420, reader.ReadValueOffset<int>() );
                var offset = reader.ReadOffset();
                Assert.AreEqual( 16, offset );
                Assert.AreEqual( 69, reader.ReadValueAtOffset<int>( offset ) );
            }
        }

        [TestMethod()]
        public void WriteOffsetTest()
        {
            var stream = new MemoryStream();
            using ( var writer = new BinaryObjectWriter( stream, StreamOwnership.Retain, Endianness.Little ) )
            {
                writer.WriteOffset( () =>
                {
                    writer.Write( 69 );
                    writer.WriteOffset( alignment: 16, action: () =>
                    {
                        writer.Write( 420 );
                    });
                });

                writer.Flush();
                Assert.AreEqual( 2, writer.OffsetHandler.OffsetPositions.Count() );
                CollectionAssert.AreEqual( new[] { 0l, 8l }, writer.OffsetHandler.OffsetPositions.ToList() );
            }

            Debug.Assert( stream.Length == 20 );
            stream.Position = 0;

            using ( var reader = new BinaryObjectReader( stream, StreamOwnership.Retain, Endianness.Little ) )
            {
                reader.ReadOffset((reader2) =>
                {
                    Assert.AreEqual( 4, reader2.Position );
                    var value = reader2.Read<int>();
                    Assert.AreEqual( 69, value );
                    reader2.ReadOffset((reader3) =>
                    {
                        Assert.AreEqual( 16, reader3.Position );
                        Assert.AreEqual( 420, reader3.Read<int>() );
                    });
                });
            }
        }
    }
}