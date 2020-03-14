using System;
using System.Collections.Generic;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Text;

namespace Amicitia.IO.Binary
{
    public interface IDynamicBinarySerializable : IBaseBinarySerializable
    {

    }
    
    public class DynamicBinarySerializableTestClass1 : IDynamicBinarySerializable
    {
        [BinaryIgnore] public BinarySourceInfo BinarySourceInfo { get; set; }
        [BinaryMember] public float X { get; set; }
        [BinaryMember] public float Y { get; set; }
        [BinaryMember] public float Z { get; set; }
        [BinaryMember] public Vector2 Position { get; set; }
        [BinaryMember( IsOffset = true )] public DynamicBinarySerializableTestClass2 Class2 { get; set; }
    }

    public class DynamicBinarySerializableTestClass2 : IDynamicBinarySerializable
    {

    }

    public class FileWaveGroup : IDynamicBinarySerializable, IBinarySourceInfo
    {
        [BinaryIgnore] public BinarySourceInfo BinarySourceInfo { get; set; }

        [BinaryMember( StringFormat = StringBinaryFormat.FixedLength, Length = 112 )] 
        public string ArchiveName { get; set; }

        [BinaryMember] private int mWaveInfoCount;  
        
        [BinaryMember( LengthMember = nameof( mWaveInfoCount ), ElementIsOffset = true )]
        public WaveInfo[] WaveInfo { get; set; }
    }

    public class WaveInfo : IDynamicBinarySerializable, IBinarySourceInfo
    {
        [BinaryIgnore] public BinarySourceInfo BinarySourceInfo { get; set; }
        public byte Field00;
        public byte Format;
        public byte RootKey;
        public byte Padding;
        public float SampleRate;
        public uint WaveStart;
        public uint WaveSize;
        public uint HasLoop;
        public uint LoopStart;
        public uint LoopEnd;
        public uint SampleCount;
        public ushort HistoryLast;
        public ushort HistoryPenult;
        public uint Field34;
        public uint Field38;
    }

    [AttributeUsage( AttributeTargets.Field | AttributeTargets.Property, Inherited = true, AllowMultiple = false )]
    public sealed class BinaryMemberAttribute : Attribute
    {
        public int LineNumber { get; }
        public int Order { get; set; }
        public bool IsOffset { get; set; }
        public int OffsetOrigin { get; set; }
        public int Size { get; set; }
        public int Length { get; set; }
        public string LengthMember { get; set; }
        public bool ElementIsOffset { get; set; }
        public int ElementSize { get; set; }
        public Encoding Encoding { get; set; }
        public int Alignment { get; set; }
        public StringBinaryFormat StringFormat { get; set; }
        public Type Converter { get; set; }

        public BinaryMemberAttribute([CallerLineNumber] int lineNumber = -1)
        {
            LineNumber = lineNumber;
            Order = -1;
            OffsetOrigin = -1;
            Size = -1;
            Length = -1;
            ElementSize = -1;
            Alignment = -1;
        }
    }

    public sealed class BinaryIgnoreAttribute : Attribute { }

    public interface IDynamicBinarySerializer
    {
        void Read( BinaryObjectReader reader );
        void Write( BinaryObjectWriter writer );
    }
}
