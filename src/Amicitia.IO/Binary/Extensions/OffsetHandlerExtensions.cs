using System;
using System.Collections.Generic;
using System.Text;

namespace Amicitia.IO.Binary.Extensions
{
    public static class OffsetHandlerExtensions
    {
        public static OffsetOriginToken WithOffsetOrigin( this IOffsetHandler handler, long origin )
        {
            return new OffsetOriginToken( handler, origin );
        }
    }
}
