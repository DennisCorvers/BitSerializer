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

        public BitStreamer WriteString(string value, FastEncoding encoding = FastEncoding.ASCII)
        {
            if (encoding == FastEncoding.ASCII)
                return WriteASCII(value);

            return WriteUTF16(value);
        }

        /// <summary>
        /// Writes 1 byte per character
        /// </summary>
        public BitStreamer WriteASCII(string value)
        {
            if (string.IsNullOrEmpty(value))
                throw new ArgumentNullException(nameof(value));

            int totalBytes = value.Length;

            if (totalBytes > ushort.MaxValue)
                throw new ArgumentOutOfRangeException("value", "String is too large to be written.");

            EnsureWriteSize((totalBytes + sizeof(ushort)) * 8);
            WriteUnchecked((ushort)totalBytes, 16);

            if (totalBytes <= 256)
            {
                byte* buffer = stackalloc byte[totalBytes];
                for (int i = 0; i < value.Length; i++)
                    buffer[i] = (byte)value[i];

                WriteMemoryUnchecked(buffer, totalBytes);
            }
            else
            {
                byte* buffer = (byte*)Memory.Alloc(totalBytes);
                try
                {
                    for (int i = 0; i < value.Length; i++)
                        buffer[i] = (byte)value[i];

                    WriteMemoryUnchecked(buffer, totalBytes);
                }
                finally
                {
                    // Ensure we don't have a mem leak.
                    Memory.Free(buffer);
                }
            }

            return this;
        }

        /// <summary>
        /// Writes 2 bytes per character.
        /// </summary>
        public BitStreamer WriteUTF16(string value)
        {
            if (string.IsNullOrEmpty(value))
                throw new ArgumentNullException(nameof(value));

            int totalBytes = value.Length * 2;

            if (totalBytes > ushort.MaxValue)
                throw new ArgumentOutOfRangeException("value", "String is too large to be written.");

            EnsureWriteSize((totalBytes + sizeof(ushort)) * 8);
            WriteUnchecked((ushort)totalBytes, 16);

            fixed (char* ptr = value)
            {
                WriteMemoryUnchecked(ptr, totalBytes);
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

        private void WriteStringInternal(char* ptr, int charCount, Encoding encoding)
        {
            const int BUFFERSIZE = 256;

            int byteSize = encoding.GetByteCount(ptr, charCount);

            if (byteSize > ushort.MaxValue)
                throw new ArgumentOutOfRangeException("value", "String is too large to be written.");

            EnsureWriteSize((byteSize + sizeof(ushort)) * 8);

            // Write the length of the string in bytes.
            WriteUnchecked((ushort)byteSize, 16);

            // Fast route, use stackalloc for small strings.
            if (byteSize <= BUFFERSIZE)
            {
                byte* buffer = stackalloc byte[byteSize];
                encoding.GetBytes(ptr, charCount, buffer, byteSize);
                WriteMemoryUnchecked(buffer, byteSize);
            }
            // Slow route, alloc mem for large strings.
            else
            {
                byte* buffer = (byte*)Memory.Alloc(byteSize);
                try
                {
                    encoding.GetBytes(ptr, charCount, buffer, byteSize);
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

        public string ReadString(FastEncoding encoding = FastEncoding.ASCII)
        {
            if (encoding == FastEncoding.ASCII)
                return ReadASCII();

            return ReadUTF16();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public string ReadASCII()
        {
            return ReadString(Encoding.ASCII);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public string ReadUTF16()
        {
            return ReadString(Encoding.Unicode);
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
        /// Reads a maximum of charLength or the original string length.
        /// </summary>
        public int ReadString(char* ptr, int ptrLength, Encoding encoding)
        {
            return ReadStringInternal(ptr, ptrLength, encoding);
        }

        private int ReadStringInternal(char* str, int charLength, Encoding encoding)
        {
            const int BUFFERSIZE = 256;
            ushort byteSize = ReadUShort();

            if (byteSize == 0)
                return 0;

            EnsureReadSize(byteSize * 8);

            // Fast route, use stackalloc for small strings.
            if (byteSize <= BUFFERSIZE)
            {
                byte* buffer = stackalloc byte[byteSize];
                return DecodeString(buffer, byteSize, str, charLength, encoding);
            }
            // Slow route, alloc mem for large strings.
            else
            {
                byte* buffer = (byte*)Memory.Alloc(byteSize);
                try
                {
                    return DecodeString(buffer, byteSize, str, charLength, encoding);
                }
                finally
                {
                    // Ensure we don't have a mem leak.
                    Memory.Free(buffer);
                }
            }
        }

        private int DecodeString(byte* buffer, int bufferSize, char* str, int maxChars, Encoding encoding)
        {
            ReadMemoryUnchecked(buffer, bufferSize);

            int maxCharCount = encoding.GetMaxCharCount(bufferSize);
            if (maxCharCount <= 128)
            {
                char* chrBuf = stackalloc char[maxCharCount];
                return InnerDecode(chrBuf);
            }
            else
            {
                char* chrBuf = (char*)Memory.Alloc(maxCharCount * 2);
                try
                {
                    return InnerDecode(chrBuf);
                }
                finally
                {
                    // Ensure we don't have a mem leak.
                    Memory.Free(buffer);
                }
            }

            int InnerDecode(char* chrBuf)
            {
                // Copy to temp buffer.
                int charCount = encoding.GetChars(buffer, bufferSize, chrBuf, maxCharCount);

                // Copy to destination buffer.
                int charsToCopy = Math.Min(charCount, maxChars);
                Memory.CopyMemory(chrBuf, str, charsToCopy * 2);

                return charsToCopy;
            }
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public BitStreamer Serialize(ref string value, Encoding encoding)
        {
            if (m_mode == SerializationMode.Writing) WriteString(value, encoding);
            else value = ReadString(encoding);
            return this;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public BitStreamer Serialize(ref string value, FastEncoding encoding = FastEncoding.ASCII)
        {
            if (m_mode == SerializationMode.Writing) WriteString(value, encoding);
            else value = ReadString(encoding);
            return this;
        }
    }
}
