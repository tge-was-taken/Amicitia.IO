using System;
using System.Runtime.CompilerServices;

namespace Amicitia.IO
{
    public unsafe ref struct SpanList<T> where T : unmanaged
    {
        private T[] mArray;

        public Span<T> Span;
        public int     Count;
        public int     Capacity => Span.Length;

        public ref T this[int index] => ref Span[index];

        public SpanList( Span<T> span )
        {
            mArray = null;
            Span   = span;
            Count  = 0;
        }

        public void Add( T value ) => Add( ref value );

        public void Add( ref T value )
        {
            EnsureCapacity( Count + 1 );
            Span[Count++] = value;
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        private void EnsureCapacity( int size )
        {
            if ( size >= Span.Length )
            {
                if ( mArray == null )
                {
                    mArray = new T[Span.Length * 2];
                    var oldSpan = Span;
                    Span = mArray;
                    oldSpan.CopyTo( Span );
                }
                else
                {
                    Array.Resize( ref mArray, Span.Length * 2 );
                }

                Span = mArray;
            }
        }

        public int IndexOf( T item )
        {
            for ( int i = 0; i < Span.Length; i++ )
                if ( Span[i].Equals( item ) ) return i;

            return -1;
        }

        public void Insert( int index, T item )
        {
            EnsureCapacity( Math.Max( index + 1, Count + 1 ) );
            for ( int i = index; i < Span.Length; i++ ) Span[index + 1] = Span[index];
            Span[index] = item;
        }

        public void RemoveAt( int index )
        {
            EnsureCapacity( Math.Max( index + 1, Count + 1 ) );
            for ( int i = index + 1; i < Span.Length; i++ ) Span[index] = Span[index + 1];
        }

        public void Clear()
        {
            Unsafe.InitBlock( Unsafe.AsPointer( ref Span[0] ), 0, ( uint )Span.Length );
        }

        public bool Contains( T item )
            => IndexOf( item ) != -1;

        public void CopyTo( T[] array, int arrayIndex )
            => throw new NotImplementedException();

        public bool Remove( T item )
        {
            var index = IndexOf( item );
            if ( index == -1 ) return false;
            RemoveAt( index );
            return true;
        }

        public Span<T>.Enumerator GetEnumerator()
            => Span.GetEnumerator();
    }
}
