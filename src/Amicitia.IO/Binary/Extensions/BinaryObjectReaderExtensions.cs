using System;
using System.Collections.Generic;
using System.Text;

namespace Amicitia.IO.Binary.Extensions
{
    public static class BinaryObjectReaderExtensions
    {
        public static void PushOffsetOrigin( this BinaryObjectReader reader )
            => reader.OffsetHandler.PushOffsetOrigin( reader.Position );

        public static void PopOffsetOrigin( this BinaryObjectReader reader )
            => reader.OffsetHandler.PopOffsetOrigin();

        public static OffsetOriginToken WithOffsetOrigin( this BinaryObjectReader reader )
            => reader.OffsetHandler.WithOffsetOrigin( reader.Position );

        public static OffsetOriginToken WithOffsetOrigin( this BinaryObjectReader reader, long origin )
         => reader.OffsetHandler.WithOffsetOrigin( origin );
    }
}
