using BitSerializer.Utils;
using System;
using System.Diagnostics;

namespace BitSerializer
{
    public unsafe partial class BitStreamer
    {
        private void WriteMemoryUnchecked(void* ptr, int byteSize)
        {
            long c = byteSize >> 3; // longs
            ulong* lp = (ulong*)ptr;
            byte* bp = (byte*)ptr;

            int i = 0;
            for (; i < c; i++)
                WriteUnchecked(lp[i], 64);

            i = i << 3;
            for (; i < byteSize; i++)
                WriteUnchecked(bp[i], 8);
        }

        private void ReadMemoryUnchecked(void* ptr, int byteSize)
        {
            long c = byteSize >> 3; // longs
            ulong* lp = (ulong*)ptr;
            byte* bp = (byte*)ptr;

            int i = 0;
            for (; i < c; i++)
                lp[i] = ReadUnchecked(64);

            i = i << 3;
            for (; i < byteSize; i++)
                bp[i] = unchecked((byte)ReadUnchecked(8));
        }

        /// <summary>
        /// Writes raw data to the <see cref="BitStreamer"/>.
        /// </summary>
        public BitStreamer WriteMemory(IntPtr ptr, int byteSize)
        {
            return WriteMemory((void*)ptr, byteSize);
        }

        /// <summary>
        /// Writes raw data to the <see cref="BitStreamer"/>.
        /// </summary>
        public BitStreamer WriteMemory(void* ptr, int byteSize)
        {
            if (byteSize < 0)
                throw new ArgumentOutOfRangeException(nameof(byteSize));

            if (ptr == null)
                throw new ArgumentNullException(nameof(ptr));

            // Make sure there is enough space for the entire memory write operation.
            if (EnsureWriteSize(byteSize * 8))
                WriteMemoryUnchecked(ptr, byteSize);

            return this;
        }

        /// <summary>
        /// Reads raw data from the <see cref="BitStreamer"/>.
        /// </summary>
        public void ReadMemory(IntPtr ptr, int byteSize)
        {
            ReadMemory((void*)ptr, byteSize);
        }

        /// <summary>
        /// Reads raw data from the <see cref="BitStreamer"/>.
        /// </summary>
        public void ReadMemory(void* ptr, int byteSize)
        {
            if (byteSize < 0)
                throw new ArgumentOutOfRangeException(nameof(byteSize));

            if (ptr == null)
                throw new ArgumentNullException(nameof(ptr));

            // Make sure there is enough space for the entire memory read operation.
            if (EnsureReadSize(byteSize))
                ReadMemoryUnchecked(ptr, byteSize);
        }

        public BitStreamer WriteBytes(byte[] bytes, bool includeSize = false)
        {
            return WriteBytes(bytes, 0, bytes.Length, includeSize);
        }

        /// <summary>
        /// Writes bytes to the <see cref="BitStreamer"/>.
        /// </summary>
        public BitStreamer WriteBytes(byte[] bytes, int offset, int count, bool includeSize = false)
        {
            if (bytes == null)
                throw new ArgumentNullException(nameof(bytes));

            if ((uint)offset + (uint)count > bytes.Length)
                throw new ArgumentOutOfRangeException("Offset and count exceed array size");

            if (includeSize)
                WriteUShort((ushort)count);

            // Make sure there is enough space for the entire memory write operation.
            if (EnsureWriteSize(count))
            {
                fixed (byte* ptr = &bytes[offset])
                {
                    WriteMemoryUnchecked(ptr, count);
                }
            }

            return this;
        }

        /// <summary>
        /// Reads an array of bytes from the <see cref="BitStreamer"/>.
        /// Length is automatically retrieved as an uint16.
        /// </summary>
        public byte[] ReadBytes()
        {
            ushort length = ReadUShort();

            return ReadBytes(length);
        }

        /// <summary>
        /// Reads an array of bytes from the <see cref="BitStreamer"/>.
        /// </summary>
        /// <param name="count">The amount of bytes to read.</param>
        public byte[] ReadBytes(int count)
        {
            if (count < 0)
                throw new ArgumentOutOfRangeException(nameof(count));

            if (!EnsureReadSize(count))
                return Array.Empty<byte>();

            byte[] val = new byte[count];

            fixed (byte* ptr = val)
            {
                ReadMemoryUnchecked(ptr, count);
            }

            return val;
        }

        /// <summary>
        /// Reads bytes from the <see cref="BitStreamer"/>.
        /// </summary>
        public void ReadBytes(byte[] bytes, int offset, int count)
        {
            if (bytes == null)
                throw new ArgumentNullException(nameof(bytes));

            if ((uint)offset + (uint)count > bytes.Length)
                throw new ArgumentOutOfRangeException("Offset and count exceed array size");

            fixed (byte* ptr = &bytes[offset])
            {
                ReadMemory(ptr, count);
            }
        }

        /// <summary>
        /// Copies the inner buffer to a supplied buffer.
        /// </summary>
        /// <param name="buffer">The destination for the data.</param>
        public void CopyTo(byte[] buffer)
        {
            CopyTo(buffer, 0, BytesUsed);
        }

        /// <summary>
        /// Copies the inner buffer to a supplied buffer.
        /// </summary>
        /// <param name="buffer">The destination for the data.</param>
        public void CopyTo(byte[] buffer, int destinationIndex)
        {
            CopyTo(buffer, destinationIndex, BytesUsed);
        }

        /// <summary>
        /// Copies the inner buffer to a supplied buffer.
        /// </summary>
        /// <param name="buffer">The destination for the data.</param>
        /// <param name="length">The total length to copy (starting from 0)</param>
        public void CopyTo(byte[] buffer, int destinationIndex, int length)
        {
            if (buffer == null)
                throw new ArgumentNullException(nameof(buffer));

            if ((uint)destinationIndex + (uint)length > buffer.Length)
                throw new ArgumentOutOfRangeException("DestinationIndex and length exceed array size");

            Memory.CopyMemory((IntPtr)m_buffer, buffer, destinationIndex, Math.Min(length, BytesUsed));
        }

        /// <summary>
        /// Copies the inner buffer to a supplied buffer.
        /// </summary>
        /// <param name="ptr">The destination for the data.</param>
        /// <param name="length">The total length to copy (starting from 0)</param>
        public void CopyTo(IntPtr ptr, int length)
        {
            if (ptr == null)
                throw new ArgumentNullException(nameof(ptr));

            Memory.CopyMemory(m_buffer, (void*)ptr, Math.Min(length, BytesUsed));
        }
    }
}
