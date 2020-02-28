using System.IO;

namespace Amicitia.IO.Binary
{
    public static class BinaryValueWriterExtensions
    {
        public static void Align( this BinaryValueWriter writer, int alignment )
            => writer.Seek( AlignmentHelper.Align( writer.Position, alignment ), SeekOrigin.Begin );

        public static void Skip( this BinaryValueWriter writer, int offset )
            => writer.Seek( offset, SeekOrigin.Current );
    }
}
