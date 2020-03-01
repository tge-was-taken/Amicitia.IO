using System;
using System.IO;

namespace Amicitia.IO.Binary
{
    public class NullStream : Stream
    {
        private long mLength;

        public override bool CanRead => true;
        public override bool CanSeek => true;
        public override bool CanWrite => false;
        public override long Length => mLength;
        public override long Position { get; set; }

        public override void Flush() { }

        public override int Read( byte[] buffer, int offset, int count ) => 0;

        public override long Seek( long offset, SeekOrigin origin )
        {
            switch ( origin )
            {
                case SeekOrigin.Begin:
                    Position = offset;
                    break;
                case SeekOrigin.Current:
                    Position += offset;
                    break;
                case SeekOrigin.End:
                    Position += offset;
                    break;
                default:
                    throw new ArgumentOutOfRangeException( nameof( origin ), origin, null );
            }

            return Position;
        }

        public override void SetLength( long value ) => mLength = value;

        public override void Write( byte[] buffer, int offset, int count ) { }
    }
}