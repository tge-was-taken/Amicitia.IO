using System;
using System.IO;
using System.Runtime.CompilerServices;

namespace Amicitia.IO.Streams
{
    public sealed class StreamSpan : Stream
    {
        private readonly Stream mBaseStream;
        private readonly long mStartPosition;
        private long mPositionOffset;
        private long mLength;

        public override bool CanRead => mBaseStream.CanRead;
        public override bool CanSeek => mBaseStream.CanSeek;
        public override bool CanWrite => mBaseStream.CanWrite;
        public override long Length => mLength;
        public override long Position
        {
            get => mPositionOffset;
            set
            {
                EnsureOffsetValid( value, value );
                mPositionOffset = value;
            }
        }

        public StreamSpan( Stream stream, long start, long length )
        {
            mBaseStream = stream;
            mStartPosition = start;
            mLength = length;
        }

        public StreamSpan( Stream stream, long start )
        {
            mBaseStream = stream;
            mStartPosition = start;
            mLength = stream.Length - start;
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public override void Flush()
        {
            mBaseStream.Flush();
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public override int Read( byte[] buffer, int offset, int count )
        {
            var temp = mBaseStream.Position;
            mBaseStream.Position = mStartPosition + mPositionOffset;
            var actualCount = Math.Min( count, (int)(mLength - mPositionOffset) );
            var read = mBaseStream.Read( buffer, offset, actualCount );
            mPositionOffset += read;
            mBaseStream.Position = temp;
            return read;
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public override long Seek( long offset, SeekOrigin origin )
        {
            switch ( origin )
            {
                case SeekOrigin.Begin:
                    EnsureOffsetValid( offset, offset );
                    mPositionOffset = offset;
                    break;
                case SeekOrigin.Current:
                case SeekOrigin.End:
                    var newOffset = mPositionOffset + offset;
                    EnsureOffsetValid( offset, newOffset );
                    mPositionOffset = newOffset;
                    break;
                default:
                    throw new ArgumentOutOfRangeException( nameof( origin ) );
            }

            return mPositionOffset;
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        private void EnsureOffsetValid( long offset, long newOffset )
        {
            if ( newOffset < 0 )
                throw new ArgumentOutOfRangeException( nameof( offset ), offset, "Attempted to seek before the start of the stream." );
            else if ( mStartPosition + newOffset > mBaseStream.Length )
                throw new ArgumentOutOfRangeException( nameof( offset ), offset, "Attempted to seek past the end of the stream." );
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public override void SetLength( long value )
        {
            var newLength = mStartPosition + value;
            if ( newLength > mBaseStream.Length )
                mBaseStream.SetLength( newLength );

            mLength = value;
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public override void Write( byte[] buffer, int offset, int count )
        {
            var temp = mBaseStream.Position;
            mBaseStream.Position = mStartPosition + mPositionOffset;
            mBaseStream.Write( buffer, offset, count );
            mPositionOffset += count;
            mBaseStream.Position = temp;
        }
    }
}
