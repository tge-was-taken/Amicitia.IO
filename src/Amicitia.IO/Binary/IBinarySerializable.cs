using System;
using System.Collections.Generic;
using System.Text;

namespace Amicitia.IO.Binary
{
    public interface IBaseBinarySerializable
    {
    }

    public interface IBinarySerializable : IBaseBinarySerializable
    {
        void Read( BinaryObjectReader reader );
        void Write( BinaryObjectWriter writer );
    }

    public interface IBinarySerializable<TContext> : IBinarySerializable
    {
#if FEATURE_DEFAULT_INTERFACE_IMPLEMENTATION
        void IBinarySerializable.Read( BinaryObjectReader reader )
            => Read( reader, default );

        void IBinarySerializable.Write( BinaryObjectWriter writer )
           => Write( writer, default );
#endif

        void Read( BinaryObjectReader reader, TContext context );
        void Write( BinaryObjectWriter writer, TContext context );
    }

    public interface IBinarySourceInfo
    {
        BinarySourceInfo BinarySourceInfo { get; set; }
    }

    public interface IBinarySerializableWithInfo 
        : IBinarySerializable, IBinarySourceInfo 
    { 
    }

    public interface IBinarySerializableWithInfo<TContext> 
        : IBinarySerializable<TContext>, IBinarySourceInfo
    {
    }
}
