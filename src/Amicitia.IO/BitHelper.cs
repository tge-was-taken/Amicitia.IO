using System.Runtime.CompilerServices;

namespace Amicitia.IO
{
    public static class BitHelper
    {
        [MethodImpl( MethodImplOptions.AggressiveInlining )] 
        public static byte Unpack( byte value, int from, int to )
             => ( byte )( ( value >> from ) & ( byte.MaxValue >> ( ( sizeof( byte ) * 8 ) - ( ( to - from ) + 1 ) ) ) );

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static ushort Unpack( ushort value, int from, int to )
            => ( ushort )( ( value >> from ) & ( ushort.MaxValue >> ( ( sizeof( ushort ) * 8 ) - ( ( to - from ) + 1 ) ) ) );

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static uint Unpack( uint value, int from, int to )
            => ( value >> from ) & ( uint.MaxValue >> ( ( sizeof( uint ) * 8 ) - ( ( to - from ) + 1 ) ) );

        [MethodImpl( MethodImplOptions.AggressiveInlining )] 
        public static ulong Unpack( ulong value, int from, int to )
            => ( value >> from ) & ( ulong.MaxValue >> ( ( sizeof( ulong ) * 8 ) - ( ( to - from ) + 1 ) ) );

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static void Pack( ref byte destination, byte value, int from, int to )
        {
            var mask = byte.MaxValue >> ( sizeof( byte ) * 8 - ( to - from ) + 1 );
            destination = ( byte )( ( destination & ~( mask << from ) ) | ( ( value & mask ) << from ) );
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static void Pack( ref ushort destination, ushort value, int from, int to )
        {
            var mask = ushort.MaxValue >> ( sizeof( ushort ) * 8 - ( to - from ) + 1 );
            destination = ( ushort )( ( destination & ~( mask << from ) ) | ( ( value & mask ) << from ) );
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static void Pack( ref uint destination, uint value, int from, int to )
        {
            var mask = uint.MaxValue >> ( sizeof( uint ) * 8 - ( to - from ) + 1 );
            destination = ( uint )( ( destination & ~( mask << from ) ) | ( ( value & mask ) << from ) );
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static void Pack( ref ulong destination, ulong value, int from, int to )
        {
            var mask = ulong.MaxValue >> ( sizeof( ulong ) * 8 - ( to - from ) + 1 );
            destination = ( ulong )( ( destination & ~( mask << from ) ) | ( ( value & mask ) << from ) );
        }
    }
}
