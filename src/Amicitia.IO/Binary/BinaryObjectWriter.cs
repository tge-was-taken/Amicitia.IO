using System;
using System.Buffers;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using Amicitia.IO.Streams;

namespace Amicitia.IO.Binary
{
    public class BinaryObjectWriter : BinaryValueWriter
    {
        protected delegate void WriteOffsetJobWriter( BinaryObjectWriter writer, object value );

        protected struct WriteOffsetCmd
        {
            public long Position;
            public long OffsetOrigin;
            public int Alignment;
            public object Instance;
            public object Value;
            public WriteOffsetJobWriter Writer;

            public WriteOffsetCmd( long position, long offsetOrigin, int alignment, object instance, object value, WriteOffsetJobWriter writer )
            {
                Position = position;
                OffsetOrigin = offsetOrigin;
                Alignment = alignment;
                Value = value;
                Instance = instance;
                Writer = writer;
            }
        }

        protected const uint PLACEHOLDER_OFFSET = 0xDEADBABE;
        protected Dictionary<object, WriteOffsetCmd> mObjectCache;
        protected Queue<WriteOffsetCmd> mCmdQueue;
        protected int mMaxPriority;
        private bool mDisposed;

        public OffsetBinaryFormat OffsetBinaryFormat { get; set; }
        public IOffsetHandler OffsetHandler { get; set; }
        public int DefaultAlignment { get; set; }

        public BinaryObjectWriter( string filePath, Endianness endianness, Encoding encoding )
            : base( filePath, endianness, encoding )
        {
            Initialize();
        }

        public BinaryObjectWriter( string filePath, FileStreamingMode fileStreamingMode, Endianness endianness, Encoding encoding, int bufferSize = DEFAULT_BLOCK_SIZE )
            : base( filePath, fileStreamingMode, endianness, encoding, bufferSize )
        {
            Initialize();
        }

        public BinaryObjectWriter( Stream stream, StreamOwnership streamOwnership, Endianness endianness,
                                   Encoding encoding = null, string fileName = null, int blockSize = DEFAULT_BLOCK_SIZE )
            : base( stream, streamOwnership, endianness, encoding, fileName, blockSize )
        {
            Initialize();
        }

        private void Initialize()
        {
            mObjectCache = new Dictionary<object, WriteOffsetCmd>();
            OffsetBinaryFormat = OffsetBinaryFormat.U32;
            OffsetHandler = new DefaultOffsetHandler( mBaseStream, OffsetZeroHandling.Invalid );
            mCmdQueue = new Queue<WriteOffsetCmd>();
            DefaultAlignment = 4;
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public void WriteValueOffset<T>( T value, int alignment = 0 ) where T : unmanaged
        {
            mCmdQueue.Enqueue( new WriteOffsetCmd( Position, OffsetHandler.OffsetOrigin, alignment, value, value,
                                                   ( w, v ) => w.Write( ( T )v ) ) );
            WriteOffset( PLACEHOLDER_OFFSET );
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public void WriteArrayOffset<T>( Memory<T> value, int alignment = 0 ) where T : unmanaged
        {
            mCmdQueue.Enqueue( new WriteOffsetCmd( Position, OffsetHandler.OffsetOrigin, alignment, value, value,
                                                   ( w, v ) => w.WriteArray( ( ( Memory<T> )v ).Span ) ) );
            WriteOffset( PLACEHOLDER_OFFSET );
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public void WriteArrayOffset<T>( T[] value, int alignment = 0 ) where T : unmanaged
        {
            mCmdQueue.Enqueue( new WriteOffsetCmd( Position, OffsetHandler.OffsetOrigin, alignment, value, value,
                                                   ( w, v ) => w.WriteArray( ( T[] )v ) ) );
            WriteOffset( PLACEHOLDER_OFFSET );
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public void WriteCollectionOffset<T>( IEnumerable<T> value, int alignment = 0 ) where T : unmanaged
        {
            mCmdQueue.Enqueue( new WriteOffsetCmd( Position, OffsetHandler.OffsetOrigin, alignment, value, value,
                                                   ( w, v ) => w.WriteCollection( ( IEnumerable<T> )v ) ) );
            WriteOffset( PLACEHOLDER_OFFSET );
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public void WriteStringOffset( StringBinaryFormat format, string value, int fixedLength = -1, int alignment = 0 )
        {
            mCmdQueue.Enqueue( new WriteOffsetCmd( Position, OffsetHandler.OffsetOrigin, alignment, value, value,
                                       ( w, v ) => w.WriteString( format, ( string )v, fixedLength ) ) );
            WriteOffset( PLACEHOLDER_OFFSET );
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public void WriteStringOffset( Encoding encoding, StringBinaryFormat format, string value, int fixedLength = -1, int alignment = 0 )
        {
            mCmdQueue.Enqueue( new WriteOffsetCmd( Position, OffsetHandler.OffsetOrigin, alignment, value, value,
                                       ( w, v ) => w.WriteString( encoding, format, ( string )v, fixedLength ) ) );
            WriteOffset( PLACEHOLDER_OFFSET );
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public void WriteStringArrayOffset( StringBinaryFormat format, string[] value, int fixedLength = -1, int alignment = 0 )
        {
            mCmdQueue.Enqueue( new WriteOffsetCmd( Position, OffsetHandler.OffsetOrigin, alignment, value, value,
                                       ( w, v ) => w.WriteStringArray( format, ( string[] )v, fixedLength ) ) );
            WriteOffset( PLACEHOLDER_OFFSET );
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public void WriteStringArrayOffset( Encoding encoding, StringBinaryFormat format, string[] value, int fixedLength = -1, int alignment = 0 )
        {
            mCmdQueue.Enqueue( new WriteOffsetCmd( Position, OffsetHandler.OffsetOrigin, alignment, value, value,
                                       ( w, v ) => w.WriteStringArray( encoding, format, ( string[] )v, fixedLength ) ) );
            WriteOffset( PLACEHOLDER_OFFSET );
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        protected void WriteOffset( long value )
        {
            if ( OffsetBinaryFormat == OffsetBinaryFormat.U32 )
                Write<uint>( ( uint )value );
            else
                Write<ulong>( ( ulong )value );
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public void WriteOffset( Action action, int alignment = 0 )
        {
            mCmdQueue.Enqueue( new WriteOffsetCmd( Position, OffsetHandler.OffsetOrigin, alignment, null, null, ( w, v ) => action() ) );
            WriteOffset( PLACEHOLDER_OFFSET );
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public void WriteOffset( Action<BinaryObjectWriter> action, int alignment = 0 )
        {
            mCmdQueue.Enqueue( new WriteOffsetCmd( Position, OffsetHandler.OffsetOrigin, alignment, null, null, ( w, v ) => action( w ) ) );
            WriteOffset( PLACEHOLDER_OFFSET );
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public void WriteOffset( int alignment, object instance, object value, Action<BinaryObjectWriter, object> action )
        {
            mCmdQueue.Enqueue( new WriteOffsetCmd( Position, OffsetHandler.OffsetOrigin, alignment, instance, value, Unsafe.As<WriteOffsetJobWriter>( action ) ) );
            WriteOffset( PLACEHOLDER_OFFSET );
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public void WriteOffset( long position, long offsetBase, int alignment, object instance, object value, Action<BinaryObjectWriter, object> action )
        {
            mCmdQueue.Enqueue( new WriteOffsetCmd( position, offsetBase, alignment, instance, value, Unsafe.As<WriteOffsetJobWriter>( action ) ) );
            WriteOffset( PLACEHOLDER_OFFSET );
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public void WriteObject<T>( T value ) where T : IBinarySerializable
            => value.Write( this );

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public void WriteObject<T, TContext>( T value, TContext context ) where T : IBinarySerializable<TContext>
            => value.Write( this, context );

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public void WriteObjectOffset<T>( T value, int alignment = 0 ) where T : IBinarySerializable
        {
            if ( value == null )
            {
                WriteOffset( OffsetHandler.NullOffset );
            }
            else
            {
                mCmdQueue.Enqueue( new WriteOffsetCmd( Position, OffsetHandler.OffsetOrigin, alignment, value, value,
                                                       ( w, v ) => w.WriteObject( ( T )v ) ) );
                WriteOffset( PLACEHOLDER_OFFSET );
            }
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public void WriteObjectOffset<T, TContext>( T value, TContext context, int alignment = 0 )
            where T : IBinarySerializable<TContext>
        {
            if ( value == null )
            {
                WriteOffset( OffsetHandler.NullOffset );
            }
            else
            {
                var temp = ( value, context );
                mCmdQueue.Enqueue( new WriteOffsetCmd( Position, OffsetHandler.OffsetOrigin, alignment, value, temp,
                                                       ( w, v ) =>
                                                       {
                                                           var temp2 = ( Tuple<T, TContext> ) v;
                                                           w.WriteObject( temp2.Item1, temp2.Item2 );
                                                       }));
                WriteOffset( PLACEHOLDER_OFFSET );
            }
        }

        public void Flush()
        {
            var positionLookup = new Dictionary<object, long>();
            while ( mCmdQueue.Count > 0 )
            {
                var cmd = mCmdQueue.Dequeue();
                if ( cmd.Instance == null || !positionLookup.TryGetValue( cmd.Instance, out var pos ) )
                {
                    pos = AlignmentHelper.Align( Position, cmd.Alignment > 0 ? cmd.Alignment : DefaultAlignment );
                    if ( cmd.Instance != null )
                        positionLookup[cmd.Instance] = pos;

                    mBaseStream.Seek( pos, SeekOrigin.Begin );
                    cmd.Writer( this, cmd.Value );
                }

                var prevPos = Position;
                mBaseStream.Seek( cmd.Position, SeekOrigin.Begin );
                OffsetHandler.RegisterOffsetPosition( cmd.Position );
                WriteOffset( OffsetHandler.CalculateOffset( pos ) );
                mBaseStream.Seek( prevPos, SeekOrigin.Begin );
            }
        }

        protected override void Dispose( bool disposing )
        {
            if ( mDisposed )
                return;

            if ( disposing )
            {
                FlushBits();
                Flush();
            }

            mDisposed = true;
            base.Dispose( disposing );
        }
    }
}