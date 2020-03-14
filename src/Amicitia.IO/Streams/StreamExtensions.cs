using System.IO;
using System.Runtime.CompilerServices;

namespace Amicitia.IO.Streams
{
    public static class StreamExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static StreamSpan Slice( this Stream stream, long start, long length )
            => new StreamSpan( stream, start, length );

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static StreamSpan Slice( this Stream stream, long start )
            => new StreamSpan( stream, start );

        public static SeekToken At( this Stream stream, long offset, SeekOrigin origin )
            => new SeekToken( stream, offset, origin );
    }
}
