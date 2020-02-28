using System.Collections.Generic;

namespace Amicitia.IO.Binary
{
    public interface IOffsetHandler
    {
        long OffsetBase { get; }
        IEnumerable<long> OffsetPositions { get; }
        long NullOffset { get; }

        void PushOffsetBase( long position );
        void PopOffsetBase();
        void RegisterOffsetPositions( IEnumerable<long> offsetPositions );
        void RegisterOffsetPosition( long offsetPosition );
        long ResolveOffset( long position, long offset );
        long ResolveOffset( long offset );
        long CalculateOffset( long position );
    }
}
