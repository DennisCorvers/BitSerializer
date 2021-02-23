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

        private string ReadASCIICompressed()
        {
            const int BUFFERSIZE = 256;
            ushort byteSize = ReadUShort();

            // Fast route, use stackalloc for small strings.
            if (byteSize <= BUFFERSIZE)
            {
                byte* buffer = stackalloc byte[byteSize];
                for (int i = 0; i < byteSize; i++)
                {
                    buffer[i] = (byte)ReadUnchecked(7);
                }

                return new string((sbyte*)buffer, 0, byteSize);
            }

            throw new NotImplementedException();
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public BitStreamer Serialize(ref string value, Encoding encoding)
        {
            if (m_mode == SerializationMode.Writing) WriteString(value, encoding);
            else value = ReadString(encoding);
            return this;
        }


        /// <summary>
        /// Writes a string to the <see cref="BitStreamer"/>.
        /// </summary>
        public BitStreamer WriteString(string value, BitEncoding encoding = BitEncoding.UTF16)
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
        public BitStreamer WriteString(char[] str, BitEncoding encoding)
        {
            return WriteString(str, 0, str.Length, encoding);
        }

        public BitStreamer WriteString(char[] str, int offset, int length, BitEncoding encoding)
        {
            if (str == null)
                throw new ArgumentNullException(nameof(str));

            if ((uint)offset + (uint)length > str.Length)
                throw new ArgumentOutOfRangeException("Offset and length exceed buffer.");

            fixed (char* ptr = &str[offset])
            {
                WriteStringInternal(ptr, length, encoding);
            }

            return this;
        }

        /// <summary>
        /// Writes a string to the <see cref="BitStreamer"/>. 
        /// Includes the bytesize as an uint16.
        /// </summary>
        public BitStreamer WriteString(char* ptr, int charCount, BitEncoding encoding)
        {
            if (ptr == null)
                throw new ArgumentNullException(nameof(ptr));

            if (charCount < 0)
                throw new ArgumentOutOfRangeException(nameof(charCount));

            WriteStringInternal(ptr, charCount, encoding);

            return this;
        }


        private void WriteStringInternal(char* ptr, int charCount, BitEncoding encoding)
        {
            switch (encoding)
            {
                case BitEncoding.ASCII:
                    WriteASCIIInternal(ptr, charCount, false);
                    return;
                case BitEncoding.ASCIICompressed:
                    WriteASCIIInternal(ptr, charCount, true);
                    return;
            }

            WriteUTF16Internal(ptr, charCount);
        }

        private void WriteASCIIInternal(char* ptr, int charCount, bool compressed)
        {
            if (charCount > ushort.MaxValue)
                throw new ArgumentOutOfRangeException("value", "String is too large to be written.");

            int size = compressed ? 7 : 8;
            EnsureWriteSize(((charCount * size) + sizeof(ushort)) * 8);
            WriteUnchecked((ushort)charCount, 16);

            for (int i = 0; i < charCount; i++)
                WriteUnchecked((byte)ptr[i], size);
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


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public string ReadString(BitEncoding encoding = BitEncoding.UTF16)
        {
            switch (encoding)
            {
                case BitEncoding.ASCII:
                    return ReadString(Encoding.ASCII);
                case BitEncoding.ASCIICompressed:
                    return ReadASCIICompressed();
            }
            return ReadString(Encoding.Unicode);
        }

        /// <summary>
        /// Reads a string from the <see cref="BitStreamer"/>.
        /// </summary>
        public int ReadString(char[] destination, BitEncoding encoding)
        {
            return ReadString(destination, 0, destination.Length, encoding);
        }

        public int ReadString(char[] destination, int offset, int length, BitEncoding encoding)
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
        public int ReadString(char* ptr, int ptrLength, BitEncoding encoding)
        {
            if (ptr == null)
                throw new ArgumentNullException(nameof(ptr));

            return ReadStringInternal(ptr, ptrLength, encoding);
        }


        private int ReadStringInternal(char* ptr, int ptrLength, BitEncoding encoding)
        {
            if (ptrLength < 0)
                throw new ArgumentOutOfRangeException(nameof(ptrLength));

            switch (encoding)
            {
                case BitEncoding.ASCII:
                    return ReadASCIIInternal(ptr, ptrLength, false);
                case BitEncoding.ASCIICompressed:
                    return ReadASCIIInternal(ptr, ptrLength, true);
            }

            return ReadUTF16Internal(ptr, ptrLength);
        }

        private int ReadASCIIInternal(char* str, int charLength, bool compressed)
        {
            ushort charCount = ReadUShort();
            int charSize = compressed ? 7 : 8;

            if (charCount == 0)
                return 0;

            EnsureReadSize(charCount * charSize);

            int toProcess = Math.Min(charCount, charLength);

            for (int i = 0; i < toProcess; i++)
                str[i] = (char)ReadUnchecked(charSize);

            // Flush
            Skip((charCount - toProcess) * charSize);

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
        public BitStreamer Serialize(ref string value, BitEncoding encoding = BitEncoding.UTF16)
        {
            if (m_mode == SerializationMode.Writing) WriteString(value, encoding);
            else value = ReadString(encoding);
            return this;
        }
    }
}
