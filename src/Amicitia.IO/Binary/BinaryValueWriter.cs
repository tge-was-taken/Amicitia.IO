using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using Amicitia.IO.Streams;

namespace Amicitia.IO.Binary
{
    public class BinaryValueWriter : IDisposable
    {
        public const int DEFAULT_BLOCK_SIZE = 1024 * 1024;
        protected const int IN_MEMORY_STREAMING_THRESHOLD = 1024 * 1024 * 10;
        protected const int TEMP_BUFFER_SIZE = 32;

        protected Stream mBaseStream;
        protected bool mLeaveOpen;
        protected int mBlockSize;
        protected byte[] mTempBuffer;
        protected byte mBits;
        protected int mBitIndex;
        private bool mDisposed;

        public virtual long Position => mBaseStream.Position;
        public long Length => mBaseStream.Length;
        public string FilePath { get; private set; }
        public Endianness Endianness { get; set; }
        public Encoding Encoding { get; private set; }

        public BinaryValueWriter( string filePath, Endianness endianness, Encoding encoding = null )
        {
            var fileStream = File.Create(filePath);
            var bufferSize = DEFAULT_BLOCK_SIZE;
            Initialize( fileStream, StreamOwnership.Transfer, filePath, endianness, encoding, bufferSize );
        }

        public BinaryValueWriter( string filePath, FileStreamingMode fileStreamingMode, Endianness endianness, Encoding encoding = null, int bufferSize = DEFAULT_BLOCK_SIZE )
        {
            var fileStream = File.Create(filePath);
            Initialize( fileStream, StreamOwnership.Transfer, filePath, endianness, encoding, bufferSize );
        }

        public BinaryValueWriter( Stream stream, StreamOwnership streamOwnership, Endianness endianness,
                                   Encoding encoding = null, string fileName = null, int blockSize = DEFAULT_BLOCK_SIZE )
            => Initialize( stream, streamOwnership, fileName, endianness, encoding, blockSize );

        public Stream GetBaseStream()
            => mBaseStream;

        public virtual void Seek( long offset, SeekOrigin origin )
        {
            FlushBits();
            mBaseStream.Seek( offset, origin );
        }


        public SeekToken At( long offset, SeekOrigin origin )
        {
            FlushBits();
            return new SeekToken( mBaseStream, offset, origin );
        }

        // Primitives
        public void WriteBit( int index, bool value )
        {
            if ( mBitIndex > 7 )
            {
                Write( mBits );
                mBitIndex = 0;
            }

            mBits = ( byte )( ( mBits & ~( 1 << index ) ) | Unsafe.As<bool, byte>( ref value ) << index );
            mBitIndex = index;
        }

        public void WriteBit( bool value )
        {
            if ( mBitIndex < 0 )
            {
                // Initialise current bits
                mBitIndex = 0;
                mBits = 0;
            }

            WriteBit( mBitIndex, value );
            mBitIndex++;
        }

        /// <summary>
        /// Write a value in the format of the the specified endianness.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="value"></param>
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public unsafe void Write<T>( T value ) where T : unmanaged
            => Write( ref value );

        public unsafe void Write<T>( ref T value ) where T : unmanaged
        {
            FlushBits();

            if ( typeof( T ) == typeof( byte ) || typeof( T ) == typeof( sbyte ) )
            {
                // Optimise for byte/sbyte
                WriteByteCore( Unsafe.As<T, byte>( ref value ) );
                return;
            }

            if ( Unsafe.SizeOf<T>() != 1 && IsSwappingNeeded() )
                BinaryOperations<T>.Reverse( ref value );

#if NETSTANDARD2_1
            WriteBytesCore( MemoryMarshal.Cast<T, byte>( MemoryMarshal.CreateReadOnlySpan( ref value, 1 ) ) );
#else
            WriteBytesCore( MemoryMarshal.Cast<T, byte>( new ReadOnlySpan<T>( Unsafe.AsPointer( ref value ), 1 ) ) );
#endif
        }

        /// <summary>
        /// Writes a value in little endian format regardless of specified or native endianness.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="value"></param>
        public unsafe void WriteLittle<T>( T value ) where T : unmanaged
        {
            FlushBits();

            if ( typeof( T ) == typeof( byte ) || typeof( T ) == typeof( sbyte ) )
            {
                // Optimise for byte/sbyte
                WriteByteCore( Unsafe.As<T, byte>( ref value ) );
                return;
            }

            if ( Unsafe.SizeOf<T>() != 1 && !BitConverter.IsLittleEndian )
                BinaryOperations<T>.Reverse( ref value );

#if NETSTANDARD2_1
            WriteBytesCore( MemoryMarshal.Cast<T, byte>( MemoryMarshal.CreateReadOnlySpan( ref value, 1 ) ) );
#else
            WriteBytesCore( MemoryMarshal.Cast<T, byte>( new ReadOnlySpan<T>( Unsafe.AsPointer( ref value ), 1 ) ) );
#endif
        }


        /// <summary>
        /// Writes a value in the platform's byte ordering format
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="value"></param>
        public unsafe void WriteNative<T>(T value) where T : unmanaged
        {
            FlushBits();

            if ( typeof( T ) == typeof( byte ) || typeof( T ) == typeof( sbyte ) )
            {
                // Optimise for byte/sbyte
                WriteByteCore( Unsafe.As<T, byte>( ref value ) );
                return;
            }

#if NETSTANDARD2_1
            WriteBytesCore( MemoryMarshal.Cast<T, byte>( MemoryMarshal.CreateReadOnlySpan( ref value, 1 ) ) );
#else
            WriteBytesCore( MemoryMarshal.Cast<T, byte>( new ReadOnlySpan<T>( Unsafe.AsPointer( ref value ), 1 ) ) );
#endif
        }

        /// <summary>
        /// Writes a value in big endian format regardless of specified or native endianness.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="value"></param>
        public unsafe void WriteBig<T>( T value ) where T : unmanaged
        {
            FlushBits();

            if ( typeof( T ) == typeof( byte ) || typeof( T ) == typeof( sbyte ) )
            {
                // Optimise for byte/sbyte
                WriteByteCore( Unsafe.As<T, byte>( ref value ) );
                return;
            }

            if ( Unsafe.SizeOf<T>() != 1 && BitConverter.IsLittleEndian )
                BinaryOperations<T>.Reverse( ref value );

#if NETSTANDARD2_1
            WriteBytesCore( MemoryMarshal.Cast<T, byte>( MemoryMarshal.CreateReadOnlySpan( ref value, 1 ) ) );
#else
            WriteBytesCore( MemoryMarshal.Cast<T, byte>( new ReadOnlySpan<T>( Unsafe.AsPointer( ref value ), 1 ) ) );
#endif
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteArray<T>( Span<T> data ) where T : unmanaged
            => WriteArray( (ReadOnlySpan<T>)data );

        public unsafe void WriteArray<T>( ReadOnlySpan<T> data ) where T : unmanaged
        {
            if ( data.Length == 0 ) return;

            FlushBits();

            var elementSize = Unsafe.SizeOf<T>();
            var size = elementSize * data.Length;
            if ( size == 1 || !IsSwappingNeeded() )
            {
                // Write bytes directly
                WriteBytesCore( MemoryMarshal.Cast<T, byte>( data ) );
            }
            else
            {
                // Swap each element then write
                for ( int i = 0; i < data.Length; i++ )
                {
                    // Make sure to make a copy here
                    // Don't want to overwrite
                    var element = data[i];
                    BinaryOperations<T>.Reverse( ref element );

#if NETSTANDARD2_1
                    WriteBytesCore( MemoryMarshal.Cast<T, byte>( MemoryMarshal.CreateReadOnlySpan( ref element, 1 ) ) );
#else
                    WriteBytesCore( MemoryMarshal.Cast<T, byte>( new ReadOnlySpan<T>( Unsafe.AsPointer( ref element ), 1 ) ) );
#endif
                }
            }
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public void WriteArray<T>( T[] array ) where T : unmanaged
            => WriteArray<T>( array.AsSpan() );

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public void WriteCollection<T>( IEnumerable<T> collection ) where T : unmanaged
        {
            foreach ( var item in collection )
                Write( item );
        }

#region Backwards compatibility / Helpers
        public void WriteSByte( sbyte value ) => Write<sbyte>( value );
        public void WriteByte( byte value ) => Write<byte>( value );
        public void WriteInt16( short value ) => Write<short>( value );
        public void WriteUInt16( ushort value ) => Write<ushort>( value );
        public void WriteInt32( int value ) => Write<int>( value );
        public void WriteUInt32( uint value ) => Write<uint>( value );
        public void WriteInt64( long value ) => Write<long>( value );
        public void WriteUInt64( ulong value ) => Write<ulong>( value );
        public void WriteSingle( float value ) => Write<float>( value );
        public void WriteDouble( double value ) => Write<double>( value );
        public void WriteBytes( byte[] bytes ) => WriteArray<byte>( bytes );
        #endregion

        //
        // -- Strings
        //
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public void WriteString( StringBinaryFormat format, string value, int fixedLength = -1 )
            => WriteString( Encoding, format, value, fixedLength );

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public void WriteStringNullTerminated( Encoding encoding, string value )
        {
            FlushBits();
            if ( string.IsNullOrEmpty( value ) )
            {
                Write<byte>(0);
                return;
            }
            var bytes = encoding.GetBytes( value );
            WriteArray( bytes );
            Write<byte>( 0 );
        }

        public void WriteStringFixedLength( Encoding encoding, string value, int fixedLength )
        {
            if ( fixedLength < 0 )
                throw new ArgumentException( "Invalid fixed length specified" );

            FlushBits();
            var bytes = encoding.GetBytes( value );
            WriteArray<byte>( bytes.AsSpan().Slice( 0, Math.Min( bytes.Length, fixedLength ) ) );

            if ( bytes.Length < fixedLength )
            {
                // Write padding
                var paddingBytes = fixedLength - bytes.Length;
                for ( int i = 0; i < paddingBytes; i++ )
                    Write<byte>( 0 );
            }
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public void WriteStringPrefixedLength8( Encoding encoding, string value )
        {
            FlushBits();
            var bytes = encoding.GetBytes( value );
            Write<byte>( ( byte )bytes.Length );
            WriteArray( bytes );
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public void WriteStringPrefixedLength16( Encoding encoding, string value )
        {
            FlushBits();
            var bytes = encoding.GetBytes( value );
            Write<ushort>( ( ushort )bytes.Length );
            WriteArray( bytes );
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public void WriteStringPrefixedLength32( Encoding encoding, string value )
        {
            FlushBits();
            var bytes = encoding.GetBytes( value );
            Write<uint>( ( uint )bytes.Length );
            WriteArray( bytes );
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public void WriteStringPrefixedLength64( Encoding encoding, string value )
        {
            FlushBits();
            var bytes = encoding.GetBytes( value );
            Write<ulong>( ( ulong )bytes.Length );
            WriteArray( bytes );
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteString( Encoding encoding, StringBinaryFormat format, string value, int fixedLength = -1 )
        {
            switch ( format )
            {
                case StringBinaryFormat.NullTerminated:
                    WriteStringNullTerminated( encoding, value );
                    break;

                case StringBinaryFormat.FixedLength:
                    WriteStringFixedLength( encoding, value, fixedLength );
                    break;

                case StringBinaryFormat.PrefixedLength8:
                    WriteStringPrefixedLength8( encoding, value );
                    break;

                case StringBinaryFormat.PrefixedLength16:
                    WriteStringPrefixedLength16( encoding, value );
                    break;

                case StringBinaryFormat.PrefixedLength32:
                    WriteStringPrefixedLength32( encoding, value );
                    break;

                case StringBinaryFormat.PrefixedLength64:
                    WriteStringPrefixedLength64( encoding, value );
                    break;

                default:
                    throw new ArgumentException( "Unknown string format", nameof( format ) );
            }

        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public void WriteStringArray( StringBinaryFormat format, string[] values, int fixedLength = -1 )
            => WriteStringArray( Encoding, format, values, fixedLength );

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public void WriteStringArray( Encoding encoding, StringBinaryFormat format, string[] values, int fixedLength = -1 )
        {
            for ( int i = 0; i < values.Length; i++ )
                WriteString( encoding, format, values[i], fixedLength );
        }

        public void Dispose()
        {
            Dispose( true );
            GC.SuppressFinalize( this );
        }

        protected virtual void Dispose( bool disposing )
        {
            if ( mDisposed )
                return;

            if ( disposing )
            {
                FlushBits();
                var positionBeforeFlushing = mBaseStream.Position;
                mBaseStream.Flush();
                if ( mBaseStream.Position != positionBeforeFlushing )
                {
                    // Fill remainder with padding
                    for ( int i = 0; i < mBaseStream.Position - positionBeforeFlushing; i++ )
                        WriteByteCore( 0 );
                }

                if ( !mLeaveOpen )
                    mBaseStream.Dispose();
            }

            mDisposed = true;
        }

        //
        // -- Private / protected
        //
        protected virtual void WriteBytesCore( Span<byte> data )
        {
            Debug.Assert( mBitIndex == -1, "Bits have not been flushed before writing" );

#if NETSTANDARD2_1
            mBaseStream.Write( data );
#else
            for (int i = 0; i < data.Length; i++)
			{
                mBaseStream.WriteByte( data[i] );
			}
#endif
        }

        protected virtual void WriteBytesCore( ReadOnlySpan<byte> data )
        {
            Debug.Assert( mBitIndex == -1, "Bits have not been flushed before writing" );

#if NETSTANDARD2_1
            mBaseStream.Write( data );
#else
            for (int i = 0; i < data.Length; i++)
			{
                mBaseStream.WriteByte( data[i] );
			}
#endif
        }

        protected virtual void WriteByteCore( byte value )
        {
            Debug.Assert( mBitIndex == -1, "Bits have not been flushed before writing" );
            mBaseStream.WriteByte( value );
        }

        protected void FlushBits()
        {
            if ( mBitIndex != -1 )
            {
                mBitIndex = -1;
                WriteByteCore( mBits );
            }
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        protected bool IsSwappingNeeded()
        {
            return BitConverter.IsLittleEndian && Endianness != Endianness.Little;
        }

        protected void Initialize( Stream input, StreamOwnership streamOwnership, string fileName, Endianness endianness, Encoding encoding, int blockSize )
        {
            mLeaveOpen = streamOwnership == StreamOwnership.Retain;
            mBlockSize = blockSize;

            // TODO: fix memory buffering
            if ( true || blockSize == 0 || input is MemoryStream )
            {
                // No block buffering
                mBaseStream = input;
                mBlockSize = 0;
            }
            else
            {
                // Block buffered
                mBaseStream = new CachedBlockBufferedStream( input, blockSize, leaveOpen: mLeaveOpen );
            }

            FilePath = fileName;
            Endianness = endianness;
            Encoding = encoding ?? Encoding.Default;
            mBitIndex = -1;
        }

        protected static Stream PrepareFileStreaming( FileStreamingMode fileStreamingMode, ref int blockSize, FileStream fileStream )
        {
            // TODO: fix memory buffering
            return fileStream;

            switch ( fileStreamingMode )
            {
                case FileStreamingMode.Buffered:
                    return fileStream;
                case FileStreamingMode.CopyToMemory:
                    {
                        blockSize = 0;
                        var temp = new MemoryStream();
                        fileStream.CopyTo( temp );
                        fileStream.Dispose();
                        temp.Position = 0;
                        return temp;
                    }
                    break;
                default:
                    throw new ArgumentOutOfRangeException( nameof( fileStreamingMode ) );
            }
        }
    }
}