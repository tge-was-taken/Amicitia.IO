using System.IO;
using System.Linq;

namespace Amicitia.IO.Binary
{
    public static class BinaryValueWriterExtensions
    {
        public static void Align(this BinaryValueWriter writer, int alignment)
            => writer.WriteCollection( Enumerable.Repeat<byte>( 0, ( int )( AlignmentHelper.Align( writer.Position, alignment ) - writer.Position ) ) );

        public static void Skip( this BinaryValueWriter writer, int offset )
            => writer.Seek( offset, SeekOrigin.Current );
    }
}
