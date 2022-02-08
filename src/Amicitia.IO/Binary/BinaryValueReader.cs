using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Linq;
using Amicitia.IO.Streams;
using System.Diagnostics;

namespace Amicitia.IO.Binary
{
    public class BinaryValueReader : IDisposable
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

        public long Position => mBaseStream.Position;
        public long Length => mBaseStream.Length;
        public string FilePath { get; protected set; }
        public Endianness Endianness { get; set; }
        public Encoding Encoding { get; protected set; }

        public BinaryValueReader( string filePath, Endianness endianness, Encoding encoding )
        {
            var fileStream = File.OpenRead(filePath);
            var bufferSize = DEFAULT_BLOCK_SIZE;
            var stream = PrepareFileStreaming( fileStream.Length < IN_MEMORY_STREAMING_THRESHOLD ? FileStreamingMode.CopyToMemory : FileStreamingMode.Buffered, ref bufferSize, fileStream );
            Initialize( stream, StreamOwnership.Transfer, filePath, endianness, encoding, bufferSize );
        }

        public BinaryValueReader( string filePath, FileStreamingMode fileStreamingMode, Endianness endianness, Encoding encoding, int bufferSize = DEFAULT_BLOCK_SIZE )
        {
            var fileStream = File.OpenRead(filePath);
            var stream = PrepareFileStreaming( fileStreamingMode, ref bufferSize, fileStream );
            Initialize( stream, StreamOwnership.Transfer, filePath, endianness, encoding, bufferSize );
        }

        public BinaryValueReader( Stream stream, StreamOwnership streamOwnership, Endianness endianness,
                                  Encoding encoding = null, string fileName = null, int blockSize = DEFAULT_BLOCK_SIZE )
            => Initialize( stream, streamOwnership, fileName, endianness, encoding ?? Encoding.Default, blockSize );

        public Stream GetBaseStream()
            => mBaseStream;

        public void Seek( long offset, SeekOrigin origin )
        {
            mBaseStream.Seek( offset, origin );
            InvalidateBits();
        }

        public SeekToken At( long offset, SeekOrigin origin )
        {
            InvalidateBits();
            return new SeekToken( mBaseStream, offset, origin );
        }

        // Primitives
        public bool ReadBit( int index )
        {
            if ( mBitIndex < 0 || mBitIndex > 7 )
                mBits = InternalReadByte();

            mBitIndex = index;
            return ( mBits & ( 1 << index ) ) != 0;
        }

        public bool ReadBit()
        {
            if ( mBitIndex < 0 || mBitIndex > 7 )
            {
                mBits = InternalReadByte();
                mBitIndex = 0;
            }

            return ( mBits & ( 1 << mBitIndex++ ) ) != 0;
        }

        public T Read<T>() where T : unmanaged
        {
            var value = ReadNative<T>();
            if ( Unsafe.SizeOf<T>() != 1 && IsSwappingNeeded() )
                BinaryOperations<T>.Reverse( ref value );

            return value;
        }

        public void Read<T>( out T value ) where T : unmanaged
        {
            ReadNative<T>( out value );
            if ( Unsafe.SizeOf<T>() != 1 && IsSwappingNeeded() )
                BinaryOperations<T>.Reverse( ref value );
        }

        /// <summary>
        /// Reads a value in little endian format regardless of the specified endianness.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T ReadLittle<T>() where T : unmanaged
        {
            var value = ReadNative<T>();
            if ( Unsafe.SizeOf<T>() != 1 && !BitConverter.IsLittleEndian )
                BinaryOperations<T>.Reverse( ref value );

            return value;
        }

        /// <summary>
        /// Reads a value in little endian format regardless of the specified endianness.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public void ReadLittle<T>( out T value ) where T : unmanaged
        {
            ReadNative<T>( out value );
            if ( Unsafe.SizeOf<T>() != 1 && !BitConverter.IsLittleEndian )
                BinaryOperations<T>.Reverse( ref value );
        }

        /// <summary>
        /// Reads a value in big endian format regardless of the specified endianness.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T ReadBig<T>() where T : unmanaged
        {
            var value = ReadNative<T>();
            if ( Unsafe.SizeOf<T>() != 1 && BitConverter.IsLittleEndian )
                BinaryOperations<T>.Reverse( ref value );

            return value;
        }

        public void ReadBig<T>( out T value ) where T : unmanaged
        {
            ReadNative<T>( out value );
            if ( Unsafe.SizeOf<T>() != 1 && BitConverter.IsLittleEndian )
                BinaryOperations<T>.Reverse( ref value );
        }


        /// <summary>
        /// Reads a value in the platform's byte ordering format
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public T ReadNative<T>() where T : unmanaged
        {
            ReadNative<T>( out var value );
            return value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void ReadNative<T>( out T value ) where T : unmanaged
        {
            if ( typeof( T ) == typeof( byte ) || typeof( T ) == typeof( sbyte ) )
            {
                // Optimise for byte/sbyte
                var tmp = InternalReadByte();
                value = Unsafe.As<byte, T>( ref tmp );
                return;
            }

            var size = Unsafe.SizeOf<T>();
            InternalReadBytes( size, out var data );

            value = Unsafe.As<byte, T>( ref data[0] );
        }

        public void ReadArray<T>( int count, ref Span<T> destination ) where T : unmanaged
        {
            if ( count == 0 ) return;

            var elementSize = Unsafe.SizeOf<T>();
            var size = elementSize * count;

            InternalReadBytes( size, out var source );
            MarshalReadBuffer( destination, elementSize, size, source );
        }

        public void ReadArray<T>( int count, Span<T> destination ) where T : unmanaged
        {
            if ( count == 0 ) return;

            var elementSize = Unsafe.SizeOf<T>();
            var size = elementSize * count;

            InternalReadBytes( size, out var source );
            MarshalReadBuffer( destination, elementSize, size, source );
        }

        public T[] ReadArray<T>( int count ) where T : unmanaged
        {
            var array = new T[count];
            var span = new Span<T>( array);
            ReadArray( count, span );
            return array;
        }

        public void ReadCollection<T>( int count, ICollection<T> destination ) where T : unmanaged
        {
            if ( count == 0 ) return;

            var elementSize = Unsafe.SizeOf<T>();
            var size = elementSize * count;
            InternalReadBytes( size, out var data );

            if ( !IsSwappingNeeded() )
            {
                for ( int i = 0; i < count; i++ )
                    destination.Add( Unsafe.As<byte, T>( ref data[i * elementSize] ) );
            }
            else
            {
                for ( int i = 0; i < count; i++ )
                {
                    var value = Unsafe.As<byte, T>( ref data[i * elementSize] );
                    BinaryOperations<T>.Reverse( ref value );
                    destination.Add( value );
                }
            }
        }

        #region Backwards compatibility / Helpers
        public byte ReadSByte() => Read<byte>();
        public byte ReadByte() => Read<byte>();
        public short ReadInt16() => Read<short>();
        public ushort ReadUInt16() => Read<ushort>();
        public int ReadInt32() => Read<int>();
        public uint ReadUInt32() => Read<uint>();
        public long ReadInt64() => Read<long>();
        public ulong ReadUInt64() => Read<ulong>();
        public float ReadSingle() => Read<float>();
        public double ReadDouble() => Read<double>();
        byte[] ReadBytes( int count ) => ReadArray<byte>( count );
        #endregion

        //
        // -- Strings
        //
        public string ReadString( StringBinaryFormat format, int fixedLength = -1 )
            => ReadString( Encoding, format, fixedLength );

        public string ReadString( Encoding encoding, StringBinaryFormat format, int fixedLength = -1 )
        {
            Span<byte> data;

            switch ( format )
            {
                case StringBinaryFormat.NullTerminated:
                    {
                        if ( mTempBuffer == null )
                            mTempBuffer = new byte[TEMP_BUFFER_SIZE];

                        int i = 0;
                        byte b;
                        while ( ( b = Read<byte>() ) != 0 )
                        {
                            if ( i >= mTempBuffer.Length )
                                Array.Resize( ref mTempBuffer, mTempBuffer.Length * 2 );

                            mTempBuffer[i++] = b;
                        }

                        data = new Span<byte>( mTempBuffer, 0, i );
                    }
                    break;

                case StringBinaryFormat.FixedLength:
                    {
                        if ( fixedLength == -1 )
                            throw new ArgumentException( "Invalid fixed length specified" );

                        InternalReadBytes( fixedLength, out var buffer );

                        // Find actual length
                        var length = buffer.Length;
                        for ( int i = 0; i < buffer.Length; i++ )
                        {
                            if ( buffer[i] == 0 )
                            {
                                length = i;
                                break;
                            }
                        }

                        data = buffer.Slice( 0, length );
                    }
                    break;

                case StringBinaryFormat.PrefixedLength8:
                    {
                        var length = Read<byte>();
                        InternalReadBytes( length, out data );
                    }
                    break;

                case StringBinaryFormat.PrefixedLength16:
                    {
                        var length = Read<ushort>();
                        InternalReadBytes( length, out data );
                    }
                    break;

                case StringBinaryFormat.PrefixedLength32:
                    {
                        var length = Read<uint>();
                        InternalReadBytes( ( int )length, out data );
                    }
                    break;

                case StringBinaryFormat.PrefixedLength64:
                    {
                        var length = Read<ulong>();
                        Debug.Assert( length <= int.MaxValue );
                        InternalReadBytes( ( int )length, out data );
                    }
                    break;

                default:
                    throw new ArgumentException( "Unknown string format", nameof( format ) );
            }

#if NETSTANDARD2_1
            return encoding.GetString( data );
#else
            return encoding.GetString( data.ToArray() );
#endif
        }

        public string[] ReadStringArray( StringBinaryFormat format, int count )
            => ReadStringArray( Encoding, format, count );

        public string[] ReadStringArray( Encoding encoding, StringBinaryFormat format, int count, int fixedLength = -1 )
        {
            // TODO: optimize
            var array = new string[count];
            for ( int i = 0; i < array.Length; i++ )
                array[i] = ReadString( encoding, format, fixedLength );

            return array;
        }

        public void ReadStringCollection( Encoding encoding, StringBinaryFormat format, int count, ICollection<string> destination, int fixedLength = -1 )
        {
            // TODO: optimize
            for ( int i = 0; i < count; i++ )
                destination.Add( ReadString( encoding, format, fixedLength ) );
        }

        public virtual void Dispose()
        {
            if ( !mLeaveOpen )
                mBaseStream.Dispose();
        }

        //
        // -- Private / protected
        //
        protected void InternalReadBytes( int size, out Span<byte> data )
        {
            if ( mBaseStream.Position + size > mBaseStream.Length )
                throw new IOException( "Attempted to read past end of stream" );

            if ( mBlockSize == 0 )
            {
                if ( mTempBuffer == null || size >= mTempBuffer.Length )
                    mTempBuffer = new byte[Math.Max( TEMP_BUFFER_SIZE, size * 2 )];

                mBaseStream.Read( mTempBuffer, 0, size );
                data = mTempBuffer;
            }
            else
            {
                // Get block data directly without copying (hopefully)
                ( ( CachedBlockBufferedStream )mBaseStream ).Read( size, out data );
            }

            InvalidateBits();
        }

        protected unsafe byte InternalReadByte()
        {
            var value = mBaseStream.ReadByte();
            if ( value == -1 ) throw new IOException( "Attempted to read past end of stream" );
            return ( byte )value;
        }

        protected unsafe void MarshalReadBuffer<T>( Span<T> destination, int elementSize, int size, Span<byte> source ) where T : unmanaged
        {
            var destinationSpan = new Span<byte>(Unsafe.AsPointer(ref destination[0]), size);
            Unsafe.CopyBlock( ref destinationSpan[0], ref source[0], ( uint )size );

            if ( elementSize != 1 && IsSwappingNeeded() )
            {
                for ( int i = 0; i < destination.Length; i++ )
                    BinaryOperations<T>.Reverse( ref destination[i] );
            }
        }

        protected void InvalidateBits()
        {
            mBitIndex = -1;
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        protected unsafe bool IsSwappingNeeded()
        {
            return BitConverter.IsLittleEndian && Endianness != Endianness.Little;
        }

        protected void Initialize( Stream input, StreamOwnership streamOwnership, string fileName, Endianness endianness, Encoding encoding, int blockSize )
        {
            mLeaveOpen = streamOwnership == StreamOwnership.Retain;
            mBlockSize = blockSize;

            if ( blockSize == 0 )
            {
                // No block buffering
                mBaseStream = input;
            }
            else
            {
                // Block buffered
                mBaseStream = new CachedBlockBufferedStream( input, blockSize, leaveOpen: mLeaveOpen );
            }

            FilePath = fileName;
            Endianness = endianness;
            Encoding = encoding;
            mBitIndex = -1;
        }

        protected static Stream PrepareFileStreaming( FileStreamingMode fileStreamingMode, ref int blockSize, FileStream fileStream )
        {
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
