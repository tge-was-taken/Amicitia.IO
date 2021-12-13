using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;
using Amicitia.IO.Streams;

namespace Amicitia.IO.Binary
{

    public class BinaryObjectReader : BinaryValueReader
    {
        protected Dictionary<long, object> mObjectCache;

        public OffsetBinaryFormat OffsetBinaryFormat { get; set; }
        public IOffsetHandler OffsetHandler { get; set; }
        public bool PopulateBinarySourceInfo { get; set; }

        public BinaryObjectReader( string filePath, Endianness endianness, Encoding encoding )  
            : base( filePath, endianness, encoding )
        {
            Initialize();
        }

        public BinaryObjectReader( string filePath, FileStreamingMode fileStreamingMode, Endianness endianness, Encoding encoding, int bufferSize = DEFAULT_BLOCK_SIZE )
            : base( filePath, fileStreamingMode, endianness, encoding, bufferSize )
        {
            Initialize();
        }

        public BinaryObjectReader( Stream  stream, StreamOwnership streamOwnership, Endianness endianness,
                                   Encoding encoding = null, string fileName = null, int blockSize = DEFAULT_BLOCK_SIZE )
            : base( stream, streamOwnership, endianness, encoding, fileName, blockSize )
        {
            Initialize();
        }

        private void Initialize()
        {
            mObjectCache = new Dictionary<long, object>();
            OffsetBinaryFormat = OffsetBinaryFormat.U32;
            OffsetHandler = new DefaultOffsetHandler( mBaseStream, OffsetZeroHandling.Invalid );
            PopulateBinarySourceInfo = true;
        }

        //
        // -- Offsets
        //
        #region Offset methods
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public long ReadOffsetValue()
        {
            return OffsetBinaryFormat == OffsetBinaryFormat.U32 ? Read<uint>() : Read<long>();
        }

        [DebuggerStepThrough]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public void ReadOffset( Action<BinaryObjectReader> action ) 
            => ReadAtOffset( ReadOffsetValue(), action );

        [DebuggerStepThrough]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public void ReadOffset( Action action ) 
            => ReadAtOffset( ReadOffsetValue(), action );

        [DebuggerStepThrough]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public SeekToken ReadOffset()
            => AtOffset( ReadOffsetValue() );

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public SeekToken AtOffset( long offset )
        {
            var target = OffsetHandler.ResolveOffset(offset);
            if ( target != -1 )
                return new SeekToken( mBaseStream, target, SeekOrigin.Begin );

            throw new InvalidOperationException( "Offset is not valid" );
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public void ReadAtOffset( long offset, Action action )
        {
            var target = OffsetHandler.ResolveOffset(offset);
            if ( target != -1 )
            {
                var positionSave = Position;
                Seek( target, SeekOrigin.Begin );
                action();
                Seek( positionSave, SeekOrigin.Begin );
            }
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public void ReadAtOffset( long offset, Action<BinaryObjectReader> action )
        {
            var target = OffsetHandler.ResolveOffset(offset);
            if ( target != -1 )
            {
                var positionSave = Position;
                Seek( target, SeekOrigin.Begin );
                action( this );
                Seek( positionSave, SeekOrigin.Begin );
            }
        }

        [DebuggerStepThrough]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public T ReadValueOffset<T>() where T : unmanaged
            => ReadValueAtOffset<T>( ReadOffsetValue() );

        [DebuggerStepThrough]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T ReadValueAtOffset<T>( SeekToken offset ) where T : unmanaged
            => ReadValueAtOffset<T>( ( long ) offset );

        public T ReadValueAtOffset<T>( long offset ) where T : unmanaged
        {
            T value = default;

            var target = OffsetHandler.ResolveOffset( offset );
            if ( target != -1 )
            {
                if ( !mObjectCache.TryGetValue( target, out var cachedValue ) )
                {
                    var positionSave = Position;
                    Seek( target, SeekOrigin.Begin );
                    value = Read<T>();
                    Seek( positionSave, SeekOrigin.Begin );
                    mObjectCache[target] = value;
                }
                else
                {
                    value = ( T )cachedValue;
                }
            }

            return value;
        }

        [DebuggerStepThrough]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public T[] ReadArrayOffset<T>( int count ) where T : unmanaged
            => ReadArrayAtOffset<T>( ReadOffsetValue(), count );

        public T[] ReadArrayAtOffset<T>( long offset, int count ) where T : unmanaged
        {
            T[] value = default;

            var target = OffsetHandler.ResolveOffset( offset );
            if ( target != -1 )
            {
                if ( !mObjectCache.TryGetValue( target, out var cachedValue ) )
                {
                    var positionSave = Position;
                    Seek( target, SeekOrigin.Begin );
                    value = ReadArray<T>( count );
                    Seek( positionSave, SeekOrigin.Begin );
                    mObjectCache[target] = value;
                }
                else
                {
                    value = ( T[] )cachedValue;
                }
            }

            return value;
        }

        [DebuggerStepThrough]
        public T ReadObjectOffset<T>() where T : IBinarySerializable, new()
            => ReadObjectAtOffset<T>( ReadOffsetValue() );

        [DebuggerStepThrough]
        public T ReadObjectOffset<T, TContext>( TContext context ) where T : IBinarySerializable<TContext>, new()
            => ReadObjectAtOffset<T, TContext>( ReadOffsetValue(), context );

        public T ReadObjectAtOffset<T>( long offset ) 
            where T : IBinarySerializable, new()
        {
            T value = default;

            var target = OffsetHandler.ResolveOffset( offset );
            if ( target != -1 )
            {
                if ( !mObjectCache.TryGetValue( target, out var cachedValue ) )
                {
                    value = new T();
                    var positionSave = Position;
                    Seek( target, SeekOrigin.Begin );
                    ReadObject( ref value );
                    Seek( positionSave, SeekOrigin.Begin );
                    mObjectCache[target] = value;
                }
                else
                {
                    value = ( T )cachedValue;
                }
            }

            return value;
        }

        public T ReadObjectAtOffset<T, TContext>( long offset, TContext context )
            where T : IBinarySerializable<TContext>, new()
        {
            T value = default;

            var target = OffsetHandler.ResolveOffset( offset );
            if (target != -1)
            {
                if ( !mObjectCache.TryGetValue( target, out var cachedValue ) )
                {
                    value = new T();
                    var positionSave = Position;
                    Seek( target, SeekOrigin.Begin );
                    ReadObject( ref value, context );
                    Seek( positionSave, SeekOrigin.Begin );
                    mObjectCache[target] = value;
                }
                else
                {
                    value = ( T )cachedValue;
                }
            }

            return value;
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public T ReadObject<T>() where T : IBinarySerializable, new()
        {
            var value = new T();
            ReadObject( ref value );
            return value;
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public void ReadObject( IBinarySerializable obj )
        {
            ReadObject( ref obj );
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public void ReadObject<T>( ref T obj ) where T : IBinarySerializable
        {
            var startOffset = Position;
            obj.Read( this );
            var endOffset = Position;

            if ( PopulateBinarySourceInfo && obj is IBinarySourceInfo info )
                info.BinarySourceInfo = new BinarySourceInfo( FilePath, startOffset, endOffset, ( int )( endOffset - startOffset ), Endianness );
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public T ReadObject<T, TContext>( TContext context ) where T : IBinarySerializable<TContext>, new()
        {
            var value = new T();
            ReadObject( ref value, context );
            return value;
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public void ReadObject<T, TContext>( ref T obj, TContext context ) where T : IBinarySerializable<TContext>
        {
            var startOffset = Position;
            obj.Read( this, context );
            var endOffset = Position;
            MaybePopulateSourceInfo( obj, startOffset, endOffset );
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        private void MaybePopulateSourceInfo( IBinarySerializable value, long startOffset, long endOffset )
        {
            if ( PopulateBinarySourceInfo && value is IBinarySourceInfo info )
                info.BinarySourceInfo = new BinarySourceInfo( FilePath, startOffset, endOffset, ( int )( endOffset - startOffset ), Endianness );
        }

        #endregion
    }
}
