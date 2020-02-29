using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;

namespace Amicitia.IO.Generics
{
    public interface IGenericNumber { }

    public static class GenericNumber<T> where T : IGenericNumber
    {
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static int Value()
        {
            if ( typeof( T ) == typeof( N0 ) ) return 0;
            else return Unsafe.SizeOf<T>();
        }
    }

     // 0 ... 63 (for bitfields)
    [StructLayout( LayoutKind.Sequential, Size = 0 )] public struct N0 : IGenericNumber { static N0() => Debug.Assert( Unsafe.SizeOf<N0>() == 1 ); }
    [StructLayout( LayoutKind.Sequential, Size = 1 )] public struct N1 : IGenericNumber { static N1() => Debug.Assert( Unsafe.SizeOf<N1>() == 1 ); }
    [StructLayout( LayoutKind.Sequential, Size = 2 )] public struct N2 : IGenericNumber { static N2() => Debug.Assert( Unsafe.SizeOf<N2>() == 2 ); }
    [StructLayout( LayoutKind.Sequential, Size = 3 )] public struct N3 : IGenericNumber { static N3() => Debug.Assert( Unsafe.SizeOf<N3>() == 3 ); }
    [StructLayout( LayoutKind.Sequential, Size = 4 )] public struct N4 : IGenericNumber { static N4() => Debug.Assert( Unsafe.SizeOf<N4>() == 4 ); }
    [StructLayout( LayoutKind.Sequential, Size = 5 )] public struct N5 : IGenericNumber { static N5() => Debug.Assert( Unsafe.SizeOf<N5>() == 5 ); }
    [StructLayout( LayoutKind.Sequential, Size = 6 )] public struct N6 : IGenericNumber { static N6() => Debug.Assert( Unsafe.SizeOf<N6>() == 6 ); }
    [StructLayout( LayoutKind.Sequential, Size = 7 )] public struct N7 : IGenericNumber { static N7() => Debug.Assert( Unsafe.SizeOf<N7>() == 7 ); }
    [StructLayout( LayoutKind.Sequential, Size = 8 )] public struct N8 : IGenericNumber { static N8() => Debug.Assert( Unsafe.SizeOf<N8>() == 8 ); }
    [StructLayout( LayoutKind.Sequential, Size = 9 )] public struct N9 : IGenericNumber { static N9() => Debug.Assert( Unsafe.SizeOf<N9>() == 9 ); }
    [StructLayout( LayoutKind.Sequential, Size = 10 )] public struct N10 : IGenericNumber { static N10() => Debug.Assert( Unsafe.SizeOf<N10>() == 10 ); }
    [StructLayout( LayoutKind.Sequential, Size = 11 )] public struct N11 : IGenericNumber { static N11() => Debug.Assert( Unsafe.SizeOf<N11>() == 11 ); }
    [StructLayout( LayoutKind.Sequential, Size = 12 )] public struct N12 : IGenericNumber { static N12() => Debug.Assert( Unsafe.SizeOf<N12>() == 12 ); }
    [StructLayout( LayoutKind.Sequential, Size = 13 )] public struct N13 : IGenericNumber { static N13() => Debug.Assert( Unsafe.SizeOf<N13>() == 13 ); }
    [StructLayout( LayoutKind.Sequential, Size = 14 )] public struct N14 : IGenericNumber { static N14() => Debug.Assert( Unsafe.SizeOf<N14>() == 14 ); }
    [StructLayout( LayoutKind.Sequential, Size = 15 )] public struct N15 : IGenericNumber { static N15() => Debug.Assert( Unsafe.SizeOf<N15>() == 15 ); }
    [StructLayout( LayoutKind.Sequential, Size = 16 )] public struct N16 : IGenericNumber { static N16() => Debug.Assert( Unsafe.SizeOf<N16>() == 16 ); }
    [StructLayout( LayoutKind.Sequential, Size = 17 )] public struct N17 : IGenericNumber { static N17() => Debug.Assert( Unsafe.SizeOf<N17>() == 17 ); }
    [StructLayout( LayoutKind.Sequential, Size = 18 )] public struct N18 : IGenericNumber { static N18() => Debug.Assert( Unsafe.SizeOf<N18>() == 18 ); }
    [StructLayout( LayoutKind.Sequential, Size = 19 )] public struct N19 : IGenericNumber { static N19() => Debug.Assert( Unsafe.SizeOf<N19>() == 19 ); }
    [StructLayout( LayoutKind.Sequential, Size = 20 )] public struct N20 : IGenericNumber { static N20() => Debug.Assert( Unsafe.SizeOf<N20>() == 20 ); }
    [StructLayout( LayoutKind.Sequential, Size = 21 )] public struct N21 : IGenericNumber { static N21() => Debug.Assert( Unsafe.SizeOf<N21>() == 21 ); }
    [StructLayout( LayoutKind.Sequential, Size = 22 )] public struct N22 : IGenericNumber { static N22() => Debug.Assert( Unsafe.SizeOf<N22>() == 22 ); }
    [StructLayout( LayoutKind.Sequential, Size = 23 )] public struct N23 : IGenericNumber { static N23() => Debug.Assert( Unsafe.SizeOf<N23>() == 23 ); }
    [StructLayout( LayoutKind.Sequential, Size = 24 )] public struct N24 : IGenericNumber { static N24() => Debug.Assert( Unsafe.SizeOf<N24>() == 24 ); }
    [StructLayout( LayoutKind.Sequential, Size = 25 )] public struct N25 : IGenericNumber { static N25() => Debug.Assert( Unsafe.SizeOf<N25>() == 25 ); }
    [StructLayout( LayoutKind.Sequential, Size = 26 )] public struct N26 : IGenericNumber { static N26() => Debug.Assert( Unsafe.SizeOf<N26>() == 26 ); }
    [StructLayout( LayoutKind.Sequential, Size = 27 )] public struct N27 : IGenericNumber { static N27() => Debug.Assert( Unsafe.SizeOf<N27>() == 27 ); }
    [StructLayout( LayoutKind.Sequential, Size = 28 )] public struct N28 : IGenericNumber { static N28() => Debug.Assert( Unsafe.SizeOf<N28>() == 28 ); }
    [StructLayout( LayoutKind.Sequential, Size = 29 )] public struct N29 : IGenericNumber { static N29() => Debug.Assert( Unsafe.SizeOf<N29>() == 29 ); }
    [StructLayout( LayoutKind.Sequential, Size = 30 )] public struct N30 : IGenericNumber { static N30() => Debug.Assert( Unsafe.SizeOf<N30>() == 30 ); }
    [StructLayout( LayoutKind.Sequential, Size = 31 )] public struct N31 : IGenericNumber { static N31() => Debug.Assert( Unsafe.SizeOf<N31>() == 31 ); }
    [StructLayout( LayoutKind.Sequential, Size = 32 )] public struct N32 : IGenericNumber { static N32() => Debug.Assert( Unsafe.SizeOf<N32>() == 32 ); }
    [StructLayout( LayoutKind.Sequential, Size = 33 )] public struct N33 : IGenericNumber { static N33() => Debug.Assert( Unsafe.SizeOf<N33>() == 33 ); }
    [StructLayout( LayoutKind.Sequential, Size = 34 )] public struct N34 : IGenericNumber { static N34() => Debug.Assert( Unsafe.SizeOf<N34>() == 34 ); }
    [StructLayout( LayoutKind.Sequential, Size = 35 )] public struct N35 : IGenericNumber { static N35() => Debug.Assert( Unsafe.SizeOf<N35>() == 35 ); }
    [StructLayout( LayoutKind.Sequential, Size = 36 )] public struct N36 : IGenericNumber { static N36() => Debug.Assert( Unsafe.SizeOf<N36>() == 36 ); }
    [StructLayout( LayoutKind.Sequential, Size = 37 )] public struct N37 : IGenericNumber { static N37() => Debug.Assert( Unsafe.SizeOf<N37>() == 37 ); }
    [StructLayout( LayoutKind.Sequential, Size = 38 )] public struct N38 : IGenericNumber { static N38() => Debug.Assert( Unsafe.SizeOf<N38>() == 38 ); }
    [StructLayout( LayoutKind.Sequential, Size = 39 )] public struct N39 : IGenericNumber { static N39() => Debug.Assert( Unsafe.SizeOf<N39>() == 39 ); }
    [StructLayout( LayoutKind.Sequential, Size = 40 )] public struct N40 : IGenericNumber { static N40() => Debug.Assert( Unsafe.SizeOf<N40>() == 40 ); }
    [StructLayout( LayoutKind.Sequential, Size = 41 )] public struct N41 : IGenericNumber { static N41() => Debug.Assert( Unsafe.SizeOf<N41>() == 41 ); }
    [StructLayout( LayoutKind.Sequential, Size = 42 )] public struct N42 : IGenericNumber { static N42() => Debug.Assert( Unsafe.SizeOf<N42>() == 42 ); }
    [StructLayout( LayoutKind.Sequential, Size = 43 )] public struct N43 : IGenericNumber { static N43() => Debug.Assert( Unsafe.SizeOf<N43>() == 43 ); }
    [StructLayout( LayoutKind.Sequential, Size = 44 )] public struct N44 : IGenericNumber { static N44() => Debug.Assert( Unsafe.SizeOf<N44>() == 44 ); }
    [StructLayout( LayoutKind.Sequential, Size = 45 )] public struct N45 : IGenericNumber { static N45() => Debug.Assert( Unsafe.SizeOf<N45>() == 45 ); }
    [StructLayout( LayoutKind.Sequential, Size = 46 )] public struct N46 : IGenericNumber { static N46() => Debug.Assert( Unsafe.SizeOf<N46>() == 46 ); }
    [StructLayout( LayoutKind.Sequential, Size = 47 )] public struct N47 : IGenericNumber { static N47() => Debug.Assert( Unsafe.SizeOf<N47>() == 47 ); }
    [StructLayout( LayoutKind.Sequential, Size = 48 )] public struct N48 : IGenericNumber { static N48() => Debug.Assert( Unsafe.SizeOf<N48>() == 48 ); }
    [StructLayout( LayoutKind.Sequential, Size = 49 )] public struct N49 : IGenericNumber { static N49() => Debug.Assert( Unsafe.SizeOf<N49>() == 49 ); }
    [StructLayout( LayoutKind.Sequential, Size = 50 )] public struct N50 : IGenericNumber { static N50() => Debug.Assert( Unsafe.SizeOf<N50>() == 50 ); }
    [StructLayout( LayoutKind.Sequential, Size = 51 )] public struct N51 : IGenericNumber { static N51() => Debug.Assert( Unsafe.SizeOf<N51>() == 51 ); }
    [StructLayout( LayoutKind.Sequential, Size = 52 )] public struct N52 : IGenericNumber { static N52() => Debug.Assert( Unsafe.SizeOf<N52>() == 52 ); }
    [StructLayout( LayoutKind.Sequential, Size = 53 )] public struct N53 : IGenericNumber { static N53() => Debug.Assert( Unsafe.SizeOf<N53>() == 53 ); }
    [StructLayout( LayoutKind.Sequential, Size = 54 )] public struct N54 : IGenericNumber { static N54() => Debug.Assert( Unsafe.SizeOf<N54>() == 54 ); }
    [StructLayout( LayoutKind.Sequential, Size = 55 )] public struct N55 : IGenericNumber { static N55() => Debug.Assert( Unsafe.SizeOf<N55>() == 55 ); }
    [StructLayout( LayoutKind.Sequential, Size = 56 )] public struct N56 : IGenericNumber { static N56() => Debug.Assert( Unsafe.SizeOf<N56>() == 56 ); }
    [StructLayout( LayoutKind.Sequential, Size = 57 )] public struct N57 : IGenericNumber { static N57() => Debug.Assert( Unsafe.SizeOf<N57>() == 57 ); }
    [StructLayout( LayoutKind.Sequential, Size = 58 )] public struct N58 : IGenericNumber { static N58() => Debug.Assert( Unsafe.SizeOf<N58>() == 58 ); }
    [StructLayout( LayoutKind.Sequential, Size = 59 )] public struct N59 : IGenericNumber { static N59() => Debug.Assert( Unsafe.SizeOf<N59>() == 59 ); }
    [StructLayout( LayoutKind.Sequential, Size = 60 )] public struct N60 : IGenericNumber { static N60() => Debug.Assert( Unsafe.SizeOf<N60>() == 60 ); }
    [StructLayout( LayoutKind.Sequential, Size = 61 )] public struct N61 : IGenericNumber { static N61() => Debug.Assert( Unsafe.SizeOf<N61>() == 61 ); }
    [StructLayout( LayoutKind.Sequential, Size = 62 )] public struct N62 : IGenericNumber { static N62() => Debug.Assert( Unsafe.SizeOf<N62>() == 62 ); }
    [StructLayout( LayoutKind.Sequential, Size = 63 )] public struct N63 : IGenericNumber { static N63() => Debug.Assert( Unsafe.SizeOf<N63>() == 63 ); }

    // Multiples of 2 up to 8192
    [StructLayout( LayoutKind.Sequential, Size = 64 )] public struct N64 : IGenericNumber { static N64() => Debug.Assert( Unsafe.SizeOf<N64>() == 64 ); }
    [StructLayout( LayoutKind.Sequential, Size = 128 )] public struct N128 : IGenericNumber { static N128() => Debug.Assert( Unsafe.SizeOf<N128>() == 128 ); }
    [StructLayout( LayoutKind.Sequential, Size = 256 )] public struct N256 : IGenericNumber { static N256() => Debug.Assert( Unsafe.SizeOf<N256>() == 256 ); }
    [StructLayout( LayoutKind.Sequential, Size = 512 )] public struct N512 : IGenericNumber { static N512() => Debug.Assert( Unsafe.SizeOf<N512>() == 512 ); }
    [StructLayout( LayoutKind.Sequential, Size = 1024 )] public struct N1024 : IGenericNumber { static N1024() => Debug.Assert( Unsafe.SizeOf<N1024>() == 1024 ); }
    [StructLayout( LayoutKind.Sequential, Size = 2048 )] public struct N2048 : IGenericNumber { static N2048() => Debug.Assert( Unsafe.SizeOf<N2048>() == 2048 ); }
    [StructLayout( LayoutKind.Sequential, Size = 4096 )] public struct N4096 : IGenericNumber { static N4096() => Debug.Assert( Unsafe.SizeOf<N4096>() == 4096 ); }
    [StructLayout( LayoutKind.Sequential, Size = 8192 )] public struct N8192 : IGenericNumber { static N8192() => Debug.Assert( Unsafe.SizeOf<N8192>() == 8192 ); }
}
