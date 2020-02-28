using System;
using System.Collections.Generic;
using System.Text;

namespace Amicitia.IO.Binary
{
    public interface IBinarySerializable
    {
        void Read( BinaryObjectReader reader );
        void Write( BinaryObjectWriter writer );
    }

    public interface IBinarySerializableWithInfo : IBinarySerializable
    {
        BinarySourceInfo BinarySourceInfo { get; set; }
    }

    public interface IBinarySerializable<TContext> : IBinarySerializable
    {
        void IBinarySerializable.Read( BinaryObjectReader reader )
            => Read( reader, default );

        void IBinarySerializable.Write( BinaryObjectWriter writer )
           => Write( writer, default );

        void Read( BinaryObjectReader reader, TContext context );
        void Write( BinaryObjectWriter writer, TContext context );
    }

    public interface IBinarySerializableWithInfo<TContext> 
        : IBinarySerializable<TContext>, IBinarySerializableWithInfo
    { 
    }
}
