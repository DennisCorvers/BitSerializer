using BlittableSerializer.Utils;
using System;
using System.Diagnostics;
using System.Text;

namespace BlittableSerializer
{
    public unsafe partial class BitStream
    {
        public const byte StringLengthMax = byte.MaxValue;

        public void WriteString(string str, Encoding encoding)
        {
            if (str == null) { throw new ArgumentNullException("str"); }

            fixed (char* ptr = str)
            { WriteString(ptr, str.Length, encoding); }
        }
        public void WriteString(char[] str, Encoding encoding)
        {
            if (str == null) { throw new ArgumentNullException("str"); }

            fixed (char* ptr = str)
            { WriteString(ptr, str.Length, encoding); }
        }
        public void WriteString(char* ptr, int charCount, Encoding encoding)
        {
            if (charCount > StringLengthMax)
            { throw new ArgumentOutOfRangeException("String length exceeds maximum allowed size of " + StringLengthMax); }

            int byteLength = encoding.GetByteCount(ptr, charCount);
            WriteUShort((ushort)byteLength);

            byte* bytes = stackalloc byte[byteLength];
            encoding.GetBytes(ptr, charCount, bytes, byteLength);
            WriteMemory(bytes, byteLength);
        }

        public string ReadString(Encoding encoding)
        {
            ushort byteCount = Math.Min(ReadUShort(), (ushort)(StringLengthMax * 4));

            if (byteCount == 0)
            { return string.Empty; }

            byte* buffer = stackalloc byte[byteCount];
            ReadMemory(buffer, byteCount);
            return new string((sbyte*)buffer, 0, byteCount, encoding);
        }
        public int ReadString(char[] destination, int offset, Encoding encoding)
        {
            Debug.Assert(offset < destination.Length, "Offset exceeds array size.");

            fixed (char* ptr = &destination[offset])
            { return ReadStringInternal(ptr, destination.Length - offset, encoding); }
        }
        public int ReadString(char* ptr, int charLength, Encoding encoding)
        {
            return ReadStringInternal(ptr, charLength, encoding);
        }
        private int ReadStringInternal(char* str, int charLength, Encoding encoding)
        {
            Debug.Assert(charLength > 0, "charLength must be at least 1.");

            ushort byteCount = Math.Min(ReadUShort(), (ushort)(StringLengthMax * 4));

            if (byteCount == 0) { return 0; }
            if (byteCount > 1024) { return byteCount; }

            byte* buffer = stackalloc byte[byteCount];
            int charCount = Math.Min(encoding.GetCharCount(buffer, byteCount), charLength);

            ReadMemory(buffer, byteCount);
            encoding.GetChars(buffer, byteCount, str, charCount);

            return charCount;
        }
    }
}
