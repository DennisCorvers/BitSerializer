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

            fixed (char* str = value)
            {
                WriteASCIIInternal(str, value.Length);
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

            fixed (char* ptr = value)
            {
                WriteUTF16Internal(ptr, value.Length);
            }

            return this;
        }

        /// <summary>
        /// Writes a string to the <see cref="BitStreamer"/>. 
        /// Includes the bytesize as an uint16.
        /// </summary>
        public BitStreamer WriteString(char[] str, FastEncoding encoding)
        {
            return WriteString(str, 0, str.Length, encoding);
        }

        public BitStreamer WriteString(char[] str, int offset, int length, FastEncoding encoding)
        {
            if (str == null)
                throw new ArgumentNullException(nameof(str));

            if ((uint)offset + (uint)length > str.Length)
                throw new ArgumentOutOfRangeException("Offset and length exceed buffer.");

            fixed (char* ptr = &str[offset])
            {
                if (encoding == FastEncoding.ASCII)
                    WriteASCIIInternal(ptr, length);
                else
                    WriteUTF16Internal(ptr, length);
            }

            return this;
        }

        /// <summary>
        /// Writes a string to the <see cref="BitStreamer"/>. 
        /// Includes the bytesize as an uint16.
        /// </summary>
        public BitStreamer WriteString(char* ptr, int charCount, FastEncoding encoding)
        {
            if (ptr == null)
                throw new ArgumentNullException(nameof(ptr));

            if (charCount < 0)
                throw new ArgumentOutOfRangeException(nameof(charCount));

            if (encoding == FastEncoding.ASCII)
                WriteASCIIInternal(ptr, charCount);
            else
                WriteUTF16Internal(ptr, charCount);

            return this;
        }

        private void WriteASCIIInternal(char* ptr, int charCount)
        {
            int totalBytes = charCount;

            if (totalBytes > ushort.MaxValue)
                throw new ArgumentOutOfRangeException("value", "String is too large to be written.");

            EnsureWriteSize((totalBytes + sizeof(ushort)) * 8);
            WriteUnchecked((ushort)totalBytes, 16);

            if (totalBytes <= 256)
            {
                byte* buffer = stackalloc byte[totalBytes];
                for (int i = 0; i < charCount; i++)
                    buffer[i] = (byte)ptr[i];

                WriteMemoryUnchecked(buffer, totalBytes);
            }
            else
            {
                byte* buffer = (byte*)Memory.Alloc(totalBytes);
                try
                {
                    for (int i = 0; i < charCount; i++)
                        buffer[i] = (byte)ptr[i];

                    WriteMemoryUnchecked(buffer, totalBytes);
                }
                finally
                {
                    // Ensure we don't have a mem leak.
                    Memory.Free(buffer);
                }
            }
        }

        private void WriteUTF16Internal(char* ptr, int charCount)
        {
            int totalBytes = charCount * 2;

            if (totalBytes > ushort.MaxValue)
                throw new ArgumentOutOfRangeException("value", "String is too large to be written.");

            EnsureWriteSize((totalBytes + sizeof(ushort)) * 8);
            WriteUnchecked((ushort)totalBytes, 16);

            WriteMemoryUnchecked(ptr, totalBytes);
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
        public int ReadString(char[] destination, FastEncoding encoding)
        {
            return ReadString(destination, 0, destination.Length, encoding);
        }

        public int ReadString(char[] destination, int offset, int length, FastEncoding encoding)
        {
            if (destination == null)
                throw new ArgumentNullException(nameof(destination));

            if ((uint)offset + (uint)length > destination.Length)
                throw new ArgumentOutOfRangeException("Offset and length exceed buffer.");


            fixed (char* ptr = &destination[offset])
            {
                return ReadStringInternal(ptr, length, encoding);
            }
        }

        /// <summary>
        /// Reads a string from the <see cref="BitStreamer"/>.
        /// Reads a maximum of charLength or the original string length.
        /// </summary>
        public int ReadString(char* ptr, int ptrLength, FastEncoding encoding)
        {
            if (ptr == null)
                throw new ArgumentNullException(nameof(ptr));

            return ReadStringInternal(ptr, ptrLength, encoding);
        }

        private int ReadStringInternal(char* ptr, int ptrLength, FastEncoding encoding)
        {
            if (ptrLength < 0)
                throw new ArgumentOutOfRangeException(nameof(ptrLength));

            if (encoding == FastEncoding.ASCII)
                return ReadASCIIInternal(ptr, ptrLength);
            else
                return ReadUTF16Internal(ptr, ptrLength);
        }

        private int ReadASCIIInternal(char* str, int charLength)
        {
            ushort byteSize = ReadUShort();

            if (byteSize == 0)
                return 0;

            EnsureReadSize(byteSize * 8);

            int toProcess = Math.Min(byteSize, charLength);

            // Read memory in chunks of 8.
            long c = toProcess >> 3; // longs
            ulong buf = 0; byte* bbuf = (byte*)&buf;

            int i = 0;
            while (c > 0)
            {
                buf = ReadUnchecked(64);

                for (int j = 0; j < 8; j++)
                    str[i++] = (char)bbuf[j];

                c--;
            }

            // Read any remaining bytes.
            while (i < toProcess)
                str[i++] = (char)ReadUnchecked(8);

            // Flush
            Skip((byteSize - toProcess) << 3);

            return toProcess;
        }

        private int ReadUTF16Internal(char* str, int charLength)
        {
            ushort byteSize = ReadUShort();

            if (byteSize == 0)
                return 0;

            EnsureReadSize(byteSize * 8);

            int toProcess = Math.Min(byteSize, charLength * 2);
            ReadMemoryUnchecked(str, toProcess);

            // Flush
            Skip((byteSize - toProcess) << 3);

            return toProcess >> 1;
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
