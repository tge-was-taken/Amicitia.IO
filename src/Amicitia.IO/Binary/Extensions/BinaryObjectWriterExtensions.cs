namespace Amicitia.IO.Binary.Extensions
{
    public static class BinaryObjectWriterExtensions
    {
        public static void PushOffsetOrigin( this BinaryObjectWriter writer )
            => writer.OffsetHandler.PushOffsetOrigin( writer.Position );

        public static void PopOffsetOrigin( this BinaryObjectWriter writer )
            => writer.OffsetHandler.PopOffsetOrigin();

        public static OffsetOriginToken WithOffsetOrigin( this BinaryObjectWriter writer )
            => writer.OffsetHandler.WithOffsetOrigin( writer.Position );

        public static OffsetOriginToken WithOffsetOrigin( this BinaryObjectWriter writer, long origin )
            => writer.OffsetHandler.WithOffsetOrigin( origin );
    }
}
