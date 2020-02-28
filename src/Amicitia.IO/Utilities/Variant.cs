using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Amicitia.IO.Utilities
{
    [StructLayout( LayoutKind.Sequential, Size = 1 )] public struct T1 { static T1() => Debug.Assert( Unsafe.SizeOf<T1>() == 1 ); }
    [StructLayout( LayoutKind.Sequential, Size = 2 )] public struct T2 { static T2() => Debug.Assert( Unsafe.SizeOf<T2>() == 2 ); }
    [StructLayout( LayoutKind.Sequential, Size = 4 )] public struct T4 { static T4() => Debug.Assert( Unsafe.SizeOf<T4>() == 4 ); }
    [StructLayout( LayoutKind.Sequential, Size = 8 )] public struct T8 { static T8() => Debug.Assert( Unsafe.SizeOf<T8>() == 8 ); }
    [StructLayout( LayoutKind.Sequential, Size = 16 )] public struct T16 { static T16() => Debug.Assert( Unsafe.SizeOf<T16>() == 16 ); }
    [StructLayout( LayoutKind.Sequential, Size = 32 )] public struct T32 { static T32() => Debug.Assert( Unsafe.SizeOf<T32>() == 32 ); }
    [StructLayout( LayoutKind.Sequential, Size = 64 )] public struct T64 { static T64() => Debug.Assert( Unsafe.SizeOf<T64>() == 64 ); }

    public unsafe struct Variant<TSize>
    {
        public static readonly int Size = Unsafe.SizeOf<TSize>();

        private TSize mValue;

        public static Variant<TSize> Create<T>( ref T value ) where T : unmanaged
        {
            if ( Unsafe.SizeOf<T>() > Unsafe.SizeOf<TSize>() ) throw new ArgumentException( "Size of type exceeds that of TSize", nameof( T ) );
            var variant = new Variant<TSize>();
            Unsafe.Copy( ref variant, Unsafe.AsPointer( ref value ) );
            return variant;
        }

        public static bool TryCreate<T>( ReadOnlySpan<T> span, out Variant<TSize> variant )
        {
            if ( ( span.Length * Unsafe.SizeOf<T>() ) > Unsafe.SizeOf<TSize>() )
            {
                variant = new Variant<TSize>();
                return false;
            }

            variant = new Variant<TSize>();
            Unsafe.Copy( ref variant, Unsafe.AsPointer( ref MemoryMarshal.GetReference( span ) ) );
            return true;
        }

        public ref T As<T>() where T : unmanaged
            => ref Unsafe.AsRef<T>( Unsafe.AsPointer( ref mValue ) );

        public Span<T> AsSpan<T>()
            => new Span<T>( Unsafe.AsPointer( ref mValue ), Unsafe.SizeOf<TSize>() );

        public Span<T> AsSpan<T>( int count )
            => new Span<T>( Unsafe.AsPointer( ref mValue ), count );
    }
}
