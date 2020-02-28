namespace Amicitia.IO.Binary
{
    public class BinarySourceInfo
    {
        public readonly string FilePath;
        public readonly long StartOffset;
        public readonly long EndOffset;
        public readonly int Size;
        public readonly Endianness Endianness;

        public BinarySourceInfo( string filePath, long startOffset, long endOffset, int size, Endianness endianness )
        {
            FilePath = filePath;
            StartOffset = startOffset;
            EndOffset = endOffset;
            Size = size;
            Endianness = endianness;
        }
    }
}
