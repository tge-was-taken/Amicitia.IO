using System;
using System.IO;

namespace Amicitia.IO.Streams
{
    public readonly struct SeekToken : IDisposable
    {
        private readonly Stream mStream; 
        private readonly long mPreviousPosition;

        public SeekToken( Stream stream, long offset, SeekOrigin origin )
        {
            mStream = stream;
            mPreviousPosition = mStream.Position;
            mStream.Seek( offset, origin );
        }

        public void Dispose()
        {
            mStream.Position = mPreviousPosition;
        }
    }
}
