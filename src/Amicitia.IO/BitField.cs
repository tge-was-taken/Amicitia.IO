using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using Amicitia.IO.Generics;
using Amicitia.IO.Utilities;

namespace Amicitia.IO
{
    public struct BitField
    {
        public readonly int From;
        public readonly int To;
        public readonly int Count;

        public BitField( int from, int to )
        {
            From = from;
            To = to;
            Count = ( to - from ) + 1;
        }

        public byte Unpack( byte value )
            => BitHelper.Unpack( value, From, To );

        public void Pack( ref byte destination, byte value )
            => BitHelper.Pack( ref destination, value, From, To );

        public ushort Unpack( ushort value )
            => BitHelper.Unpack( value, From, To );

        public void Pack( ref ushort destination, ushort value )
            => BitHelper.Pack( ref destination, value, From, To );

        public uint Unpack( uint value )
            => BitHelper.Unpack( value, From, To );

        public void Pack( ref uint destination, uint value )
            => BitHelper.Pack( ref destination, value, From, To );
    }

    public struct BitField<TFrom, TTo>
        where TFrom : IGenericNumber
        where TTo : IGenericNumber
    {
        public byte Unpack( byte value )
            => BitHelper.Unpack( value, Unsafe.SizeOf<TFrom>(), Unsafe.SizeOf<TTo>() );

        public void Pack( ref byte destination, byte value )
            => BitHelper.Pack( ref destination, value, Unsafe.SizeOf<TFrom>(), Unsafe.SizeOf<TTo>() );

        public ushort Unpack( ushort value )
            => BitHelper.Unpack( value, Unsafe.SizeOf<TFrom>(), Unsafe.SizeOf<TTo>() );

        public void Pack( ref ushort destination, ushort value )
            => BitHelper.Pack( ref destination, value, Unsafe.SizeOf<TFrom>(), Unsafe.SizeOf<TTo>() );

        public uint Unpack( uint value )
            => BitHelper.Unpack( value, Unsafe.SizeOf<TFrom>(), Unsafe.SizeOf<TTo>() );

        public void Pack( ref uint destination, uint value )
            => BitHelper.Pack( ref destination, value, Unsafe.SizeOf<TFrom>(), Unsafe.SizeOf<TTo>() );
    }

    public struct BitField<TUnderlying, TValue, TFrom, TTo>
        where TFrom : IGenericNumber
        where TTo : IGenericNumber
    {
        public TUnderlying Packed;

        [MethodImpl( MethodImplOptions.AggressiveInlining )] public TValue Get() => UnsafeEx.As<TUnderlying, TValue>( BitHelper.Unpack<TUnderlying>( ref Packed, GenericNumber<TFrom>.Value(), GenericNumber<TTo>.Value() ) );
        [MethodImpl( MethodImplOptions.AggressiveInlining )] public void Set( TValue value ) => BitHelper.Pack<TUnderlying>( ref Packed, Unsafe.As<TValue, TUnderlying>( ref value ), GenericNumber<TFrom>.Value(), GenericNumber<TTo>.Value() );

        [MethodImpl( MethodImplOptions.AggressiveInlining )] public static implicit operator byte( BitField<TUnderlying, TValue, TFrom, TTo> value ) => UnsafeEx.As<TValue, byte>( value.Get() );
        [MethodImpl( MethodImplOptions.AggressiveInlining )] public static implicit operator sbyte( BitField<TUnderlying, TValue, TFrom, TTo> value ) => Number.Cast<TValue, sbyte>( value.Get() );
        [MethodImpl( MethodImplOptions.AggressiveInlining )] public static implicit operator short( BitField<TUnderlying, TValue, TFrom, TTo> value ) => Number.Cast<TValue, short>( value.Get() );
        [MethodImpl( MethodImplOptions.AggressiveInlining )] public static implicit operator ushort( BitField<TUnderlying, TValue, TFrom, TTo> value ) => Number.Cast<TValue, ushort>( value.Get() );
        [MethodImpl( MethodImplOptions.AggressiveInlining )] public static implicit operator int( BitField<TUnderlying, TValue, TFrom, TTo> value ) => Number.Cast<TValue, int>( value.Get() );
        [MethodImpl( MethodImplOptions.AggressiveInlining )] public static implicit operator uint( BitField<TUnderlying, TValue, TFrom, TTo> value ) => Number.Cast<TValue, uint>( value.Get() );
        [MethodImpl( MethodImplOptions.AggressiveInlining )] public static implicit operator long( BitField<TUnderlying, TValue, TFrom, TTo> value ) => Number.Cast<TValue, long>( value.Get() );
        [MethodImpl( MethodImplOptions.AggressiveInlining )] public static implicit operator ulong( BitField<TUnderlying, TValue, TFrom, TTo> value ) => Number.Cast<TValue, ulong>( value.Get() );

        public override string ToString()
        {
            return Get().ToString();
        }
    }

    public struct BitField<TValue, TFrom, TTo>
        where TFrom : IGenericNumber
        where TTo : IGenericNumber
    {
        public TValue Packed;

        [MethodImpl( MethodImplOptions.AggressiveInlining )] public TValue Get() => ( BitHelper.Unpack<TValue>( ref Packed, GenericNumber<TFrom>.Value(), GenericNumber<TTo>.Value() ) );
        [MethodImpl( MethodImplOptions.AggressiveInlining )] public void Set( TValue value ) => BitHelper.Pack<TValue>( ref Packed, value, GenericNumber<TFrom>.Value(), GenericNumber<TTo>.Value() );

        [MethodImpl( MethodImplOptions.AggressiveInlining )] public static implicit operator byte( BitField<TValue, TFrom, TTo> value ) => UnsafeEx.As<TValue, byte>( value.Get() );
        [MethodImpl( MethodImplOptions.AggressiveInlining )] public static implicit operator sbyte( BitField<TValue, TFrom, TTo> value ) => Number.Cast<TValue, sbyte>( value.Get() );
        [MethodImpl( MethodImplOptions.AggressiveInlining )] public static implicit operator short( BitField<TValue, TFrom, TTo> value ) => Number.Cast<TValue, short>( value.Get() );
        [MethodImpl( MethodImplOptions.AggressiveInlining )] public static implicit operator ushort( BitField<TValue, TFrom, TTo> value ) => Number.Cast<TValue, ushort>( value.Get() );
        [MethodImpl( MethodImplOptions.AggressiveInlining )] public static implicit operator int( BitField<TValue, TFrom, TTo> value ) => Number.Cast<TValue, int>( value.Get() );
        [MethodImpl( MethodImplOptions.AggressiveInlining )] public static implicit operator uint( BitField<TValue, TFrom, TTo> value ) => Number.Cast<TValue, uint>( value.Get() );
        [MethodImpl( MethodImplOptions.AggressiveInlining )] public static implicit operator long( BitField<TValue, TFrom, TTo> value ) => Number.Cast<TValue, long>( value.Get() );
        [MethodImpl( MethodImplOptions.AggressiveInlining )] public static implicit operator ulong( BitField<TValue, TFrom, TTo> value ) => Number.Cast<TValue, ulong>( value.Get() );

        public override string ToString()
        {
            return Get().ToString();
        }
    }
}
