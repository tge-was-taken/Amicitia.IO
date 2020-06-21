using System;
using System.Runtime.CompilerServices;
using Amicitia.IO.Generics;
using Amicitia.IO.Utilities;

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
            var bits = ( to - from ) + 1;
            var shift = ( sizeof( byte ) * 8 ) - bits;
            var mask = byte.MaxValue >> shift;
            destination = ( byte )( ( destination & ~( mask << from ) ) | ( ( value & mask ) << from ) );
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static void Pack( ref ushort destination, ushort value, int from, int to )
        {
            var bits = ( to - from ) + 1;
            var shift = ( sizeof( ushort ) * 8 ) - bits;
            var mask = ushort.MaxValue >> shift;
            destination = ( ushort )( ( destination & ~( mask << from ) ) | ( ( value & mask ) << from ) );
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static void Pack( ref uint destination, uint value, int from, int to )
        {
            var bits = ( to - from ) + 1;
            var shift = ( sizeof( uint ) * 8 ) - bits;
            var mask = uint.MaxValue >> shift;
            destination = ( uint )( ( destination & ~( mask << from ) ) | ( ( value & mask ) << from ) );
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static void Pack( ref ulong destination, ulong value, int from, int to )
        {
            var bits = ( to - from ) + 1;
            var shift = ( sizeof( ulong ) * 8 ) - bits;
            var mask = ulong.MaxValue >> shift;
            destination = ( ulong )( ( destination & ~( mask << from ) ) | ( ( value & mask ) << from ) );
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static TValue Unpack<TValue>( ref TValue value, int from, int to )
        {
            if ( typeof( TValue ) == typeof( sbyte ) || typeof( TValue ) == typeof( byte ) )
            {
                return UnsafeEx.As<byte, TValue>( BitHelper.Unpack( Unsafe.As<TValue, byte>( ref value ), from, to ) );
            }
            else if ( typeof( TValue ) == typeof( short ) || typeof( TValue ) == typeof( ushort ) )
            {
                return UnsafeEx.As<ushort, TValue>( BitHelper.Unpack( Unsafe.As<TValue, ushort>( ref value ), from, to ) );
            }
            else if ( typeof( TValue ) == typeof( int ) || typeof( TValue ) == typeof( uint ) )
            {
                return UnsafeEx.As<uint, TValue>( BitHelper.Unpack( Unsafe.As<TValue, uint>( ref value ), from, to ) );
            }
            else if ( typeof( TValue ) == typeof( long ) || typeof( TValue ) == typeof( ulong ) )
            {
                return UnsafeEx.As<ulong, TValue>( BitHelper.Unpack( Unsafe.As<TValue, ulong>( ref value ), from, to ) );
            }
            else throw new ArgumentException( nameof( TValue ) );
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static void Pack<TValue>( ref TValue destination, TValue value, int from, int to )
        {
            if ( typeof( TValue ) == typeof( sbyte ) || typeof( TValue ) == typeof( byte ) )
            {
                Pack( ref Unsafe.AsRef( Unsafe.As<TValue, byte>( ref destination ) ), Unsafe.As<TValue, byte>( ref value ), from, to );
            }
            else if ( typeof( TValue ) == typeof( short ) || typeof( TValue ) == typeof( ushort ) )
            {
                Pack( ref Unsafe.AsRef( Unsafe.As<TValue, ushort>( ref destination ) ), Unsafe.As<TValue, ushort>( ref value ), from, to );
            }
            else if ( typeof( TValue ) == typeof( int ) || typeof( TValue ) == typeof( uint ) )
            {
                Pack( ref Unsafe.AsRef( Unsafe.As<TValue, uint>( ref destination ) ), Unsafe.As<TValue, uint>( ref value ), from, to );
            }
            else if ( typeof( TValue ) == typeof( long ) || typeof( TValue ) == typeof( ulong ) )
            {
                Pack( ref Unsafe.AsRef( Unsafe.As<TValue, ulong>( ref destination ) ), Unsafe.As<TValue, ulong>( ref value ), from, to );
            }
            else throw new ArgumentException( nameof( TValue ), "Invalid type specified" );
        }
    }
}
