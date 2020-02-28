using System.IO;

namespace Amicitia.IO.Binary
{
    public static class BinaryValueReaderExtensions
    {
        public static void Align( this BinaryValueReader reader, int alignment )
            => reader.Seek( AlignmentHelper.Align( reader.Position, alignment ), SeekOrigin.Begin );

        public static void Skip( this BinaryValueReader reader, int offset )
            => reader.Seek( offset, SeekOrigin.Current );
    }
}
