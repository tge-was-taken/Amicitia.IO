using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using System.Linq;

namespace Amicitia.IO.Streams
{
    /// <summary>
    /// Stream optimized for usecases where there are many sequential reads and writes at different positions in the stream.
    /// </summary>
    public class CachedBlockBufferedStream : Stream
    {
        public const int DEFAULT_BLOCK_SIZE = 1024 * 1024;
        public const int DEFAULT_MAX_BLOCK_COUNT = 100;

        private class Block
        {
            public long Start;
            public long End;
            public byte[] Data;
            public bool Flushed;
            public int UsedBytes;
        }

        private readonly Stream mBaseStream;
        private readonly Dictionary<int, Block> mBlocks;
        private Block mCurrentBlock;
        private int mCurrentBlockIndex;
        private int mCurrentBlockOffset;
        private long mPosition;
        private Lazy<byte[]> mEmptyBlockData;
        private long mLength;
        private bool mLeaveOpen;

        public override bool CanRead => true;
        public override bool CanSeek => true;
        public override bool CanWrite => true;
        public override long Length => mLength;
        public override long Position
        {
            get => mPosition;
            set => Seek(value, SeekOrigin.Begin);
        }

        public int BlockSize { get; }

        public int MaxBlockCount { get; }

        public CachedBlockBufferedStream( Stream stream, int blockSize = DEFAULT_BLOCK_SIZE, int maxBlockCount = DEFAULT_MAX_BLOCK_COUNT, bool leaveOpen = true )
        {
            mBaseStream = stream;
            BlockSize = blockSize;
            MaxBlockCount = maxBlockCount;
            mLeaveOpen = leaveOpen;
            mBlocks = new Dictionary<int, Block>();
            mEmptyBlockData = new Lazy<byte[]>();
            mLength = mBaseStream.Length;
            SetCurrentBlock( 0 );
        }

        public override void Flush()
        {
            var positionSave = mBaseStream.Position;
            void FlushBlock( Block block )
            {
                if ( block.Start > mBaseStream.Length )
                {
                    // Write padding blocks to fill the missing space
                    var paddingStartOffset = ( (  block.Start - mBaseStream.Length ) / BlockSize ) * BlockSize;
                    var paddingBlockCount = block.Start - paddingStartOffset;
                    mBaseStream.Seek( paddingStartOffset, SeekOrigin.Begin );

                    for ( int i = 0; i < paddingBlockCount; i++ )
                        mBaseStream.Write( mEmptyBlockData.Value, 0, mEmptyBlockData.Value.Length );          
                }

                mBaseStream.Seek( block.Start, SeekOrigin.Begin );
                mBaseStream.Write( block.Data, 0, block.UsedBytes );
                block.Flushed = true;
            }

            if ( !mCurrentBlock.Flushed )
                FlushBlock( mCurrentBlock );

            foreach ( var block in mBlocks.Values )
            {
                if ( !block.Flushed )
                    FlushBlock( block );
            }

            mBaseStream.Position = positionSave;
        }

        public override int ReadByte()
        {
            if ( mPosition + 1 > mBaseStream.Length )
                return -1;

            var value = mCurrentBlock.Data[mCurrentBlockOffset];
            MoveCursor( 1 );

            if ( ( mCurrentBlockOffset ) >= BlockSize )
                SetCurrentBlock( mCurrentBlockIndex + 1 );

            return value;
        }

        public int ReadBlock( int count, out Span<byte> data )
        {
            EnsureReadWithinStreamBounds( count );

            if ( ( mCurrentBlockOffset + count ) <= BlockSize )
            {
                data = new Span<byte>( mCurrentBlock.Data, mCurrentBlockOffset, count );
                MoveCursor( count );
                return count;
            }
            else
            {
                var curBlockSize = Math.Min(BlockSize - mCurrentBlockOffset, count );
                if ( curBlockSize <= 0 )
                {
                    SetCurrentBlock( mCurrentBlockIndex + 1 );
                    curBlockSize = Math.Min( BlockSize, count  );
                }

                data = new Span<byte>( mCurrentBlock.Data, mCurrentBlockOffset, curBlockSize );
                MoveCursor( curBlockSize );
                return curBlockSize;
            }
        }

        public int Read( int count, out Span<byte> data )
        {
            var bytesLeft = mBaseStream.Length - mPosition;
            ReadBlock( count, out var block );
            if ( block.Length == count || block.Length == bytesLeft )
            {
                // Avoid copy by returning buffer
                data = block;
                return block.Length;
            }

            // Create new buffer and read remaining bytes
            var buffer = new byte[count];
            data = buffer;
            Unsafe.CopyBlock( ref buffer[0], ref block[0], (uint)block.Length );
            return Read( buffer, block.Length, count - block.Length );
        }

        public override int Read( byte[] buffer, int offset, int count )
        {
            var bytesLeft = mBaseStream.Length - mPosition;
            var totalBytesRead = ReadBlock( count, out var block );
            Unsafe.CopyBlock( ref buffer[offset], ref block[0], ( uint )block.Length );

            // We still have more to read
            while ( totalBytesRead < count && totalBytesRead != bytesLeft )
            {
                ReadBlock( count - totalBytesRead, out block );
                Unsafe.CopyBlock( ref buffer[offset + totalBytesRead], ref block[0], ( uint )block.Length );
                totalBytesRead += block.Length;
            }

            return count;
        }

        public override long Seek( long offset, SeekOrigin origin )
        {
            long calculatedPosition;
            switch ( origin )
            {
                case SeekOrigin.Begin:
                    calculatedPosition = offset;
                    break;
                case SeekOrigin.Current:
                    calculatedPosition = mPosition + offset;
                    break;
                case SeekOrigin.End:
                    calculatedPosition = mPosition + offset;
                    break;
                default:
                    throw new ArgumentOutOfRangeException( nameof( origin ) );
            }

            if ( calculatedPosition < mCurrentBlock.Start || calculatedPosition >= mCurrentBlock.End )
                SetCurrentBlock( GetBlockIndex( calculatedPosition ) );

            var relOffset = ( int )( calculatedPosition - mCurrentBlock.Start );
            mCurrentBlockOffset = relOffset;
            mPosition = calculatedPosition;
            Debug.Assert( mCurrentBlock.Start + mCurrentBlockOffset == mPosition );
            return mPosition;
        }

        public override void SetLength( long value )
        {
            mLength = value;
            mBaseStream.SetLength( value );
        }

        public override void Write( byte[] buffer, int offset, int count )
        {
            if ( ( mCurrentBlockOffset + count ) <= BlockSize )
            {
                // Write fits within current block
                Unsafe.CopyBlock( ref mCurrentBlock.Data[mCurrentBlockOffset], ref buffer[offset], ( uint )count );
                mCurrentBlock.Flushed = false;
                MoveCursor( count );
                UpdateLength();
                mCurrentBlock.UsedBytes = Math.Max( mCurrentBlock.UsedBytes, mCurrentBlockOffset );
            }
            else
            {
                // Write does not fit within the current block
                var partitions = 1 + ( count / BlockSize ) + ( count % BlockSize > 0 ? 1 : 0 );
                var readBytes = 0;

                for ( int i = 0; i < partitions; i++ )
                {
                    var curBlockSize = Math.Min( BlockSize - mCurrentBlockOffset, count - readBytes );
                    if ( curBlockSize <= 0 )
                    {
                        SetCurrentBlock( mCurrentBlockIndex + 1 );
                        curBlockSize = Math.Min( BlockSize, count - readBytes );
                    }

                    Unsafe.CopyBlock( ref mCurrentBlock.Data[mCurrentBlockOffset], ref buffer[offset + readBytes], ( uint )curBlockSize );
                    mCurrentBlock.Flushed = false;
                    MoveCursor( curBlockSize );
                    UpdateLength();
                    mCurrentBlock.UsedBytes = Math.Max( mCurrentBlock.UsedBytes, mCurrentBlockOffset );
                }
            }
        }

        protected override void Dispose(bool disposing)
        {
            if ( disposing && !mLeaveOpen )
                mBaseStream.Dispose();

            base.Dispose( disposing );
        }

        private void UpdateLength()
        {
            mLength = Math.Max( mBaseStream.Length, Math.Max( mLength, mPosition + mCurrentBlockOffset ) );
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        private int GetBlockIndex( long position )
        {
            return ( int )( position / BlockSize );
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void SetCurrentBlock( int index )
        {
            if ( !mBlocks.TryGetValue( index, out var block ) )
            {
                if ( mBlocks.Count >= MaxBlockCount )
                    mBlocks.Remove( mBlocks.Keys.First() );

                block = new Block();
                block.Start = index * BlockSize;
                block.End = block.Start + BlockSize;
                block.Data = new byte[BlockSize];
                mBaseStream.Seek( block.Start, SeekOrigin.Begin );
                mBaseStream.Read( block.Data, 0, block.Data.Length );
                mBlocks[index] = block;
            }

            mCurrentBlock = block;
            mCurrentBlockIndex = index;
            mCurrentBlockOffset = 0;
            mPosition = mCurrentBlock.Start;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void MoveCursor( int offset )
        {
            mCurrentBlockOffset += offset;
            mPosition += offset;

            if ( mPosition < mCurrentBlock.Start || mPosition >= mCurrentBlock.End )
                SetCurrentBlock( GetBlockIndex( mPosition ) );
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        private void EnsureReadWithinStreamBounds( int size )
        {
            if ( mPosition + size > mBaseStream.Length )
                throw new IOException( "Attempted to read past the end of the stream" );
        }
    }
}
