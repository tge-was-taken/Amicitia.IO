using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Linq.Expressions;
using System.Reflection;
using System.Buffers.Binary;

namespace Amicitia.IO.Binary.Utilities
{
    public delegate void TypeBinaryReverseMethod<T>( ref T value );

    public static class TypeBinaryReverseMethodGenerator
    {
        private static readonly MethodInfo sReverseMethod;

        static TypeBinaryReverseMethodGenerator()
        {
            sReverseMethod = typeof(TypeBinaryReverseMethodGenerator)
                .GetMethod(nameof(Reverse), BindingFlags.Static | BindingFlags.NonPublic);
        }

        public static TypeBinaryReverseMethod<T> Generate<T>() where T : unmanaged
        {
            var type = typeof(T);
            var bodyExpressions = new List<Expression>();
            var valueExpr = Expression.Parameter(type.MakeByRefType(), "value");
            Generate( type, sReverseMethod, bodyExpressions, valueExpr );
            var body = Expression.Block( bodyExpressions );
            var lambda = Expression.Lambda<TypeBinaryReverseMethod<T>>( body, valueExpr );
            var compiled = lambda.Compile();
            return compiled;
        }

        private static void Generate( Type type, MethodInfo reverseMethod, List<Expression> body, Expression instance )
        {
            var fields = type.GetFields(  BindingFlags.Instance | BindingFlags.Public );
            foreach ( var member in fields )
            {
                var memberAccessExpr = Expression.MakeMemberAccess( instance, member );

                if ( member.FieldType == typeof( short ) || member.FieldType == typeof( ushort ) ||
                     member.FieldType == typeof( int ) || member.FieldType == typeof( uint ) ||
                     member.FieldType == typeof( long ) || member.FieldType == typeof( ulong ) ||
                     member.FieldType == typeof( float ) || member.FieldType == typeof( double ) ||
                     member.FieldType == typeof( decimal ) || member.FieldType.IsEnum )
                {
                    var typedReverseMethod = reverseMethod.MakeGenericMethod( member.FieldType );
                    body.Add( Expression.Call( typedReverseMethod, memberAccessExpr ) );
                }
                else if ( member.FieldType != typeof( byte ) && member.FieldType != typeof( sbyte ) )
                {
                    Generate( member.FieldType, reverseMethod, body, memberAccessExpr );
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void Reverse<T>( ref T value ) where T : unmanaged
        {
            // Manually inlined BinaryOperations<T>.Reverse( value )
            if ( typeof( T ) == typeof( byte ) || typeof( T ) == typeof( sbyte ) )
                return;
            else if ( typeof( T ) == typeof( short ) || typeof( T ) == typeof( ushort ) || ( Unsafe.SizeOf<T>() == sizeof( short ) && value is Enum ) )
            {
                var reversedValue = BinaryPrimitives.ReverseEndianness( Unsafe.As<T, ushort>( ref value ) );
                value = Unsafe.As<ushort, T>( ref reversedValue );
            }
            else if (typeof(T) == typeof(int) || typeof(T) == typeof(uint) || typeof(T) == typeof(float) || (Unsafe.SizeOf<T>() == sizeof(int) && value is Enum))
            {
                var reversedValue = BinaryPrimitives.ReverseEndianness( Unsafe.As<T, uint>( ref value ) );
                value = Unsafe.As<uint, T>( ref reversedValue );
            }
            else if ( typeof( T ) == typeof( long ) || typeof( T ) == typeof( ulong ) || typeof( T ) == typeof( double ) || ( Unsafe.SizeOf<T>() == sizeof(long) && value is Enum ))
            {
                var reversedValue = BinaryPrimitives.ReverseEndianness( Unsafe.As<T, ulong>( ref value ) );
                value = Unsafe.As<ulong, T>( ref reversedValue );
            }
        }
    }
}
