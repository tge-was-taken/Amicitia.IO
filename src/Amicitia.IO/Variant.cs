using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Amicitia.IO
{
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
