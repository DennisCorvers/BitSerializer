using BitSerializer.Utils;
using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text;

namespace BitSerializer
{
    public unsafe partial class BitStreamer
    {
        /// <summary>
        /// Writes a string to the <see cref="BitStreamer"/>. 
        /// Includes the bytesize as an uint16.
        /// </summary>
        public BitStreamer WriteString(string value, Encoding encoding)
        {
            if (string.IsNullOrEmpty(value))
                throw new ArgumentNullException(nameof(value));

            fixed (char* ptr = value)
            {
                WriteStringInternal(ptr, value.Length, encoding);
            }

            return this;
        }

        /// <summary>
        /// Writes a string to the <see cref="BitStreamer"/>. 
        /// Includes the bytesize as an uint16.
        /// </summary>
        public BitStreamer WriteString(char[] str, Encoding encoding)
        {
            if (str == null)
                throw new ArgumentNullException(nameof(str));

            fixed (char* ptr = str)
            {
                WriteStringInternal(ptr, str.Length, encoding);
            }

            return this;
        }

        /// <summary>
        /// Writes a string to the <see cref="BitStreamer"/>. 
        /// Includes the bytesize as an uint16.
        /// </summary>
        public BitStreamer WriteString(char* ptr, int charCount, Encoding encoding)
        {
            if (ptr == null)
                throw new ArgumentNullException(nameof(ptr));

            if (charCount < 0)
                throw new ArgumentOutOfRangeException(nameof(charCount));

            WriteStringInternal(ptr, charCount, encoding);
            return this;
        }

        private void WriteStringInternal(char* ptr, int charSize, Encoding encoding)
        {
            const int BUFFERSIZE = 256;

            int byteSize = encoding.GetByteCount(ptr, charSize);

            if (byteSize > ushort.MaxValue)
                throw new ArgumentOutOfRangeException("value", "String is too large to be written.");

            EnsureWriteSize((byteSize + sizeof(ushort)) * 8);

            // Write the length of the string in bytes.
            WriteUnchecked((ushort)byteSize, 16);

            // Fast route, use stackalloc for small strings.
            if (byteSize <= BUFFERSIZE)
            {
                byte* buffer = stackalloc byte[byteSize];
                encoding.GetBytes(ptr, charSize, buffer, byteSize);
                WriteMemoryUnchecked(buffer, byteSize);
            }
            // Slow route, alloc mem for large strings.
            else
            {
                byte* buffer = (byte*)Memory.Alloc(byteSize);
                try
                {
                    encoding.GetBytes(ptr, charSize, buffer, byteSize);
                    WriteMemoryUnchecked(ptr, byteSize);
                }
                finally
                {
                    // Ensure we don't have a mem leak.
                    Memory.Free(buffer);
                }
            }
        }

        /// <summary>
        /// Reads a string from the <see cref="BitStreamer"/>.
        /// </summary>
        public string ReadString(Encoding encoding)
        {
            const int BUFFERSIZE = 256;
            ushort byteSize = ReadUShort();

            if (byteSize == 0)
                return string.Empty;

            EnsureReadSize(byteSize * 8);

            // Fast route, use stackalloc for small strings.
            if (byteSize <= BUFFERSIZE)
            {
                byte* buffer = stackalloc byte[byteSize];
                ReadMemoryUnchecked(buffer, byteSize);
                return new string((sbyte*)buffer, 0, byteSize, encoding);
            }
            // Slow route, alloc mem for large strings.
            else
            {
                byte* buffer = (byte*)Memory.Alloc(byteSize);
                try
                {
                    ReadMemoryUnchecked(buffer, byteSize);
                    return new string((sbyte*)buffer, 0, byteSize, encoding);
                }
                finally
                {
                    // Ensure we don't have a mem leak.
                    Memory.Free(buffer);
                }
            }
        }

        /// <summary>
        /// Reads a string from the <see cref="BitStreamer"/>.
        /// </summary>
        public int ReadString(char[] destination, int offset, Encoding encoding)
        {
            if ((uint)offset >= destination.Length)
                throw new ArgumentOutOfRangeException("Offset exceeds array size.");


            fixed (char* ptr = &destination[offset])
            {
                return ReadStringInternal(ptr, destination.Length - offset, encoding);
            }
        }

        /// <summary>
        /// Reads a string from the <see cref="BitStreamer"/>.
        /// </summary>
        public int ReadString(char* ptr, int charLength, Encoding encoding)
        {
            return ReadStringInternal(ptr, charLength, encoding);
        }

        private int ReadStringInternal(char* str, int charLength, Encoding encoding)
        {
            ushort byteCount = Math.Min(ReadUShort(), (ushort)(256 * 4));

            if (byteCount == 0)
                return 0;

            byte* buffer = stackalloc byte[byteCount];
            int charCount = Math.Min(encoding.GetCharCount(buffer, byteCount), charLength);

            ReadMemory(buffer, byteCount);
            encoding.GetChars(buffer, byteCount, str, charCount);

            return charCount;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public BitStreamer Serialize(ref string value, Encoding encoding)
        {
            if (m_mode == SerializationMode.Writing) WriteString(value, encoding);
            else value = ReadString(encoding);
            return this;
        }
    }
}
