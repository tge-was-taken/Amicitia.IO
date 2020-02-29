using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace Amicitia.IO.Utilities
{
    public static class UnsafeEx
    {
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static TTo As<TFrom, TTo>( TFrom value )
            => Unsafe.As<TFrom, TTo>( ref value );
    }
}
