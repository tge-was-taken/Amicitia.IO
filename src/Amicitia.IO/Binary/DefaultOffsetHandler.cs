using System.Collections.Generic;
using System.IO;

namespace Amicitia.IO.Binary
{
    public enum OffsetZeroHandling
    {
        Invalid,
        Valid
    }

    public sealed class DefaultOffsetHandler : IOffsetHandler
    {
        private Stream mStream;
        private Stack<long> mOffsetOriginStack;
        private HashSet<long> mValidOffsetPositions;
        private OffsetZeroHandling mZeroHandling;

        public long OffsetOrigin => mOffsetOriginStack.Peek();

        public IEnumerable<long> OffsetPositions => mValidOffsetPositions;

        public long NullOffset => 0;

        public DefaultOffsetHandler( Stream stream, OffsetZeroHandling zeroHandling = OffsetZeroHandling.Invalid )
        {
            mStream = stream;
            mOffsetOriginStack = new Stack<long>();
            mOffsetOriginStack.Push( 0 );
            mValidOffsetPositions = new HashSet<long>();
        }

        public void PushOffsetOrigin( long position )
        {
            mOffsetOriginStack.Push( position );
        }

        public void PopOffsetOrigin()
        {
            mOffsetOriginStack.Pop();
        }

        public void RegisterOffsetPositions( IEnumerable<long> offsetPositions )
        {
            foreach ( var item in offsetPositions )
                mValidOffsetPositions.Add( item );
        }

        public long ResolveOffset( long position, long offset )
        {
            if ( mValidOffsetPositions.Count != 0 && !mValidOffsetPositions.Contains( position ) ) return -1;
            return ResolveOffset( offset );
        }

        public long ResolveOffset( long offset )
        {
            if ( mZeroHandling == OffsetZeroHandling.Invalid && offset == 0 ) 
                return -1;

            var position = mOffsetOriginStack.Peek() + offset;
            if ( position < 0 || position > mStream.Length ) 
                return -1;

            return position;
        }

        public long CalculateOffset( long position )
        {
            return position - OffsetOrigin;
        }

        public long CalculateOffset( long position, long origin )
        {
            return position - origin;
        }

        public void RegisterOffsetPosition( long offsetPosition )
        {
            mValidOffsetPositions.Add( offsetPosition );
        }
    }
}
