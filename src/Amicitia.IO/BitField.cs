using System;
using System.Collections.Generic;
using System.Text;

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
}
