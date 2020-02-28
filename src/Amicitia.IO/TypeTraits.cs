using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace Amicitia.IO
{
    /// <summary>
    /// Compile time type traits.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public static class TypeTraits<T>
    {
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static bool IsPrimitiveType()
        {
            return
                typeof( T ) == typeof( byte ) || typeof( T ) == typeof( sbyte ) ||
                typeof( T ) == typeof( short ) || typeof( T ) == typeof( ushort ) ||
                typeof( T ) == typeof( int ) || typeof( T ) == typeof( uint ) ||
                typeof( T ) == typeof( long ) || typeof( T ) == typeof( ulong ) ||
                typeof( T ) == typeof( float ) || typeof( T ) == typeof( double );
        }
    }
}
