using System.Runtime.CompilerServices;

namespace Amicitia.IO
{
    public static class AlignmentHelper
    {
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static byte Align( byte value, int alignment )
            => ( byte )( ( value + ( alignment - 1 ) ) & ~( alignment - 1 ) );

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static short Align( short value, int alignment )
            => ( short )( ( value + ( alignment - 1 ) ) & ~( alignment - 1 ) );

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static int Align( int value, int alignment )
            => ( value + ( alignment - 1 ) ) & ~( alignment - 1 );

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static long Align( long value, int alignment )
            => ( value + ( alignment - 1 ) ) & ~( alignment - 1 );

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static int GetAlignedDifference( byte value, int alignment )
            => ( ( value + ( alignment - 1 ) ) & ~( alignment - 1 ) ) - value;

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static int GetAlignedDifference( short value, int alignment )
            => ( ( value + ( alignment - 1 ) ) & ~( alignment - 1 ) ) - value;

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static int GetAlignedDifference( int value, int alignment )
            => ( ( value + ( alignment - 1 ) ) & ~( alignment - 1 ) ) - value;

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static int GetAlignedDifference( long value, int alignment )
            => ( int )( ( ( value + ( alignment - 1 ) ) & ~( alignment - 1 ) ) - value );
    }
}
