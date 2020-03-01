using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Amicitia.IO.Binary
{
    public readonly struct OffsetOriginToken : IDisposable
    {
        private readonly IOffsetHandler mHandler;

        public OffsetOriginToken( IOffsetHandler handler, long origin )
        {
            mHandler = handler;
            mHandler.PushOffsetOrigin( origin );
        }

        public void Dispose()
        {
            mHandler.PopOffsetOrigin();
        }
    }
}
