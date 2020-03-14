using System.Collections.Generic;

namespace Amicitia.IO.Binary
{
    public interface IOffsetHandler
    {
        long OffsetOrigin { get; }
        IEnumerable<long> OffsetPositions { get; }
        long NullOffset { get; }

        void PushOffsetOrigin( long position );
        void PopOffsetOrigin();
        void RegisterOffsetPositions( IEnumerable<long> offsetPositions );
        void RegisterOffsetPosition( long offsetPosition );
        long ResolveOffset( long position, long offset );
        long ResolveOffset( long offset );
        long CalculateOffset( long position );
        long CalculateOffset( long position, long origin );
    }
}
