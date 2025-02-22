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
            public int Priority;
            public bool PopulateInfo;
            public WriteOffsetJobWriter Writer;

            public WriteOffsetCmd( long position, long offsetOrigin, int alignment, object instance, object value, int priority, bool populateInfo, WriteOffsetJobWriter writer )
            {
                Position = position;
                OffsetOrigin = offsetOrigin;
                Alignment = alignment;
                Value = value;
                Instance = instance;
                Priority = priority;
                PopulateInfo = populateInfo;
                Writer = writer;
            }
        }

        protected const uint PLACEHOLDER_OFFSET = 0xDEADBABE;
        protected Dictionary<object, WriteOffsetCmd> mObjectCache;
        protected Queue<WriteOffsetCmd> mLinearCmdQueue;
        protected List<WriteOffsetCmd> mRecursiveCmdList;
        private bool mDisposed;

        public OffsetBinaryFormat OffsetBinaryFormat { get; set; }
        public OffsetFlushMode OffsetFlushMode { get; set; }
        public IOffsetHandler OffsetHandler { get; set; }
        public int DefaultAlignment { get; set; }
        public bool PopulateBinarySourceInfo { get; set; }

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
            OffsetFlushMode = OffsetFlushMode.Linear;
            OffsetHandler = new DefaultOffsetHandler( mBaseStream, OffsetZeroHandling.Invalid );
            mLinearCmdQueue = new Queue<WriteOffsetCmd>();
            mRecursiveCmdList = new List<WriteOffsetCmd>();
            DefaultAlignment = 4;
            PopulateBinarySourceInfo = true;
        }

        private void AddWriteOffsetCmd( in WriteOffsetCmd cmd )
        {
            if ( OffsetFlushMode == OffsetFlushMode.Recursive )
            {
                mRecursiveCmdList.Add( cmd );
            }
            else
            {
                mLinearCmdQueue.Enqueue( cmd );
            }
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public void WriteValueOffset<T>( T value, int alignment = 0, int priority = 0 ) where T : unmanaged
        {
            AddWriteOffsetCmd( new WriteOffsetCmd( Position, OffsetHandler.OffsetOrigin, alignment, value, value, priority, false,
                                                   ( w, v ) => w.Write( ( T )v ) ) );
            WriteOffsetValue( PLACEHOLDER_OFFSET );
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public void WriteArrayOffset<T>( Memory<T> value, int alignment = 0, int priority = 0 ) where T : unmanaged
        {
            AddWriteOffsetCmd( new WriteOffsetCmd( Position, OffsetHandler.OffsetOrigin, alignment, value, value, priority, false,
                                                   ( w, v ) => w.WriteArray( ( ( Memory<T> )v ).Span ) ) );
            WriteOffsetValue( PLACEHOLDER_OFFSET );
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public void WriteArrayOffset<T>( T[] value, int alignment = 0, int priority = 0 ) where T : unmanaged
        {
            if ( value == null )
            {
                WriteOffsetValue( OffsetHandler.NullOffset );
                return;
            }
            AddWriteOffsetCmd( new WriteOffsetCmd( Position, OffsetHandler.OffsetOrigin, alignment, value, value, priority, false,
                                                   ( w, v ) => w.WriteArray( ( T[] )v ) ) );
            WriteOffsetValue( PLACEHOLDER_OFFSET );
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public void WriteCollectionOffset<T>( IEnumerable<T> value, int alignment = 0, int priority = 0 ) where T : unmanaged
        {
            if ( value == null )
            {
                WriteOffsetValue( OffsetHandler.NullOffset );
                return;
            }
            AddWriteOffsetCmd( new WriteOffsetCmd( Position, OffsetHandler.OffsetOrigin, alignment, value, value, priority, false,
                                                   ( w, v ) => w.WriteCollection( ( IEnumerable<T> )v ) ) );
            WriteOffsetValue( PLACEHOLDER_OFFSET );
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public void WriteStringOffset( StringBinaryFormat format, string value, int fixedLength = -1, int alignment = 0, int priority = 0 )
        {
            if ( value == null )
            {
                WriteOffsetValue( OffsetHandler.NullOffset );
                return;
            }
            
            AddWriteOffsetCmd( new WriteOffsetCmd( Position, OffsetHandler.OffsetOrigin, alignment, value, value, priority, false,
                                       ( w, v ) => w.WriteString( format, ( string )v, fixedLength ) ) );
            WriteOffsetValue( PLACEHOLDER_OFFSET );
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public void WriteStringOffset( Encoding encoding, StringBinaryFormat format, string value, int fixedLength = -1, int alignment = 0, int priority = 0 )
        {
            AddWriteOffsetCmd( new WriteOffsetCmd( Position, OffsetHandler.OffsetOrigin, alignment, value, value, priority, false,
                                       ( w, v ) => w.WriteString( encoding, format, ( string )v, fixedLength ) ) );
            WriteOffsetValue( PLACEHOLDER_OFFSET );
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public void WriteStringArrayOffset( StringBinaryFormat format, string[] value, int fixedLength = -1, int alignment = 0, int priority = 0 )
        {
            AddWriteOffsetCmd( new WriteOffsetCmd( Position, OffsetHandler.OffsetOrigin, alignment, value, value, priority, false,
                                       ( w, v ) => w.WriteStringArray( format, ( string[] )v, fixedLength ) ) );
            WriteOffsetValue( PLACEHOLDER_OFFSET );
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public void WriteStringArrayOffset( Encoding encoding, StringBinaryFormat format, string[] value, int fixedLength = -1, int alignment = 0, int priority = 0 )
        {
            AddWriteOffsetCmd( new WriteOffsetCmd( Position, OffsetHandler.OffsetOrigin, alignment, value, value, priority, false,
                                       ( w, v ) => w.WriteStringArray( encoding, format, ( string[] )v, fixedLength ) ) );
            WriteOffsetValue( PLACEHOLDER_OFFSET );
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        protected void WriteOffsetValue( long value )
        {
            if ( OffsetBinaryFormat == OffsetBinaryFormat.U32 )
                Write<uint>( ( uint )value );
            else
                Write<ulong>( ( ulong )value );
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public void WriteOffset( Action action, int alignment = 0, int priority = 0 )
        {
            AddWriteOffsetCmd( new WriteOffsetCmd( Position, OffsetHandler.OffsetOrigin, alignment, null, null, priority, false, ( w, v ) => action() ) );
            WriteOffsetValue( PLACEHOLDER_OFFSET );
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public void WriteOffset( Action<BinaryObjectWriter> action, int alignment = 0, int priority = 0 )
        {
            AddWriteOffsetCmd( new WriteOffsetCmd( Position, OffsetHandler.OffsetOrigin, alignment, null, null, priority, false, ( w, v ) => action( w ) ) );
            WriteOffsetValue( PLACEHOLDER_OFFSET );
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public void WriteOffset( int alignment, object instance, object value, Action<BinaryObjectWriter, object> action, int priority = 0 )
        {
            AddWriteOffsetCmd( new WriteOffsetCmd( Position, OffsetHandler.OffsetOrigin, alignment, instance, value, priority, false, Unsafe.As<WriteOffsetJobWriter>( action ) ) );
            WriteOffsetValue( PLACEHOLDER_OFFSET );
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public void WriteOffset( long position, long offsetBase, int alignment, object instance, object value, Action<BinaryObjectWriter, object> action, int priority = 0 )
        {
            AddWriteOffsetCmd( new WriteOffsetCmd( position, offsetBase, alignment, instance, value, priority, false, Unsafe.As<WriteOffsetJobWriter>( action ) ) );
            WriteOffsetValue( PLACEHOLDER_OFFSET );
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public void WriteObject<T>( T value ) where T : IBinarySerializable
            => value.Write( this );

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public void WriteObject<T, TContext>( T value, TContext context ) where T : IBinarySerializable<TContext>
            => value.Write( this, context );

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public void WriteObjectOffset<T>( T value, int alignment = 0, int priority = 0 ) where T : IBinarySerializable
        {
            if ( value == null )
            {
                WriteOffsetValue( OffsetHandler.NullOffset );
            }
            else
            {
                AddWriteOffsetCmd( new WriteOffsetCmd( Position, OffsetHandler.OffsetOrigin, alignment, value, value, priority,
                    PopulateBinarySourceInfo && value is IBinarySourceInfo,
                    ( w, v ) => w.WriteObject( ( T )v ) ) );
                WriteOffsetValue( PLACEHOLDER_OFFSET );
            }
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public void WriteObjectOffset<T, TContext>( T value, TContext context, int alignment = 0, int priority = 0 )
            where T : IBinarySerializable<TContext>
        {
            if ( value == null )
            {
                WriteOffsetValue( OffsetHandler.NullOffset );
            }
            else
            {
                var temp = new Tuple<T, TContext>( value, context );
                AddWriteOffsetCmd( new WriteOffsetCmd( Position, OffsetHandler.OffsetOrigin, alignment, value, temp, priority,
                    PopulateBinarySourceInfo && value is IBinarySourceInfo,
                    ( w, v ) =>
                    {
                        var temp2 = ( Tuple<T, TContext> ) v;
                        w.WriteObject( temp2.Item1, temp2.Item2 );
                    }));
                WriteOffsetValue( PLACEHOLDER_OFFSET );
            }
        }

        private void Flush( in WriteOffsetCmd cmd, Dictionary<object, long> positionLookup )
        {
            if ( cmd.Instance == null || !positionLookup.TryGetValue( cmd.Instance, out var pos ) )
            {
                pos = AlignmentHelper.Align( Position, cmd.Alignment > 0 ? cmd.Alignment : DefaultAlignment );
                if ( cmd.Instance != null )
                    positionLookup[cmd.Instance] = pos;
            
                mBaseStream.Seek( pos, SeekOrigin.Begin );
                cmd.Writer( this, cmd.Value );
            
                if ( cmd.PopulateInfo )
                {
                    ( ( ( IBinarySourceInfo )cmd.Instance ) ).BinarySourceInfo =
                        new BinarySourceInfo( FilePath, pos, Position, ( int )( Position - pos ), Endianness );
                }
            }
            
            var prevPos = Position;
            mBaseStream.Seek( cmd.Position, SeekOrigin.Begin );
            OffsetHandler.RegisterOffsetPosition( cmd.Position );
            WriteOffsetValue( OffsetHandler.CalculateOffset( pos, cmd.OffsetOrigin ) );
            mBaseStream.Seek( prevPos, SeekOrigin.Begin );
        }

        private void FlushLinearly( Dictionary<object, long> positionLookup )
        {
            while ( mLinearCmdQueue.Count > 0 )
            { 
                Flush( mLinearCmdQueue.Dequeue(), positionLookup );
            }
        }

        private void FlushRecursively( Dictionary<object, long> positionLookup, int first, int last )
        {
            int priority = 0;
            int nextPriority;
            bool foundHigherPriorityCommand;

            do
            {
                nextPriority = int.MaxValue;
                foundHigherPriorityCommand = false;

                for ( int i = first; i < last; i++ )
                {
                    var cmd = mRecursiveCmdList[i];
                    if ( cmd.Priority == priority )
                    {
                        int firstChild = mRecursiveCmdList.Count;
                        Flush( cmd, positionLookup );
                        int lastChild = mRecursiveCmdList.Count;

                        if ( firstChild != lastChild )
                            FlushRecursively( positionLookup, firstChild, lastChild );
                    }
                    else if ( cmd.Priority > priority )
                    {
                        nextPriority = Math.Min( nextPriority, cmd.Priority );
                        foundHigherPriorityCommand = true;
                    }
                }

                priority = nextPriority;
            } while ( foundHigherPriorityCommand );
        }

        public void Flush()
        {
            var positionLookup = new Dictionary<object, long>();

            if ( OffsetFlushMode == OffsetFlushMode.Recursive )
            {
                FlushRecursively( positionLookup, 0, mRecursiveCmdList.Count );
                mRecursiveCmdList.Clear();
            }
            else
            {
                FlushLinearly( positionLookup );
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