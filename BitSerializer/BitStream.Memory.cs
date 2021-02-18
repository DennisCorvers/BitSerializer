using BitSerializer.Utils;
using System;
using System.Diagnostics;

namespace BitSerializer
{
    public unsafe partial class BitStreamer
    {
        /// <summary>
        /// Writes raw data to the <see cref="BitStreamer"/>.
        /// </summary>
        public BitStreamer WriteMemory(IntPtr ptr, int byteSize)
        {
            WriteMemory((void*)ptr, byteSize);
            return this;
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

            int numLongs = byteSize >> 3;
            ulong* p = (ulong*)ptr;

            for (int i = 0; i < numLongs; i++)
                Write(p[i], 64);

            byte* bptr = (byte*)&p[numLongs];
            for (int i = byteSize - numLongs * 8; i > 0; i--)
            {
                WriteByte(*bptr);
                bptr++;
            }

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

            int numLongs = byteSize >> 3;

            ulong* p = (ulong*)ptr;
            for (int i = 0; i < numLongs; i++)
                p[i] = Read(64);

            byte* bptr = (byte*)&p[numLongs];
            for (int i = byteSize - numLongs * sizeof(ulong); i > 0; i--)
            {
                *bptr = ReadByte();
                bptr++;
            }
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

            fixed (byte* ptr = &bytes[offset])
            {
                WriteMemory(ptr, count);
            }

            return this;
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
                ReadMemory(ptr, count); ;
            }
        }

        public byte[] ReadBytesLength()
        {
            ushort length = ReadUShort();
            byte[] val = new byte[length];
            ReadBytes(val, 0, length);
            return val;
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

            Memory.CopyMemory(new IntPtr(m_buffer), 0, buffer, destinationIndex, Math.Min(length, BytesUsed));
        }


        /// <summary>
        /// Copies the inner buffer to a supplied buffer.
        /// </summary>
        /// <param name="buffer">The destination for the data.</param>
        public void CopyTo(IntPtr ptr)
        {
            CopyTo(ptr, 0, BytesUsed);
        }

        /// <summary>
        /// Copies the inner buffer to a supplied buffer.
        /// </summary>
        /// <param name="buffer">The destination for the data.</param>
        public void CopyTo(IntPtr ptr, int destinationIndex)
        {
            CopyTo(ptr, destinationIndex, BytesUsed);
        }

        /// <summary>
        /// Copies the inner buffer to a supplied buffer.
        /// </summary>
        /// <param name="buffer">The destination for the data.</param>
        /// <param name="length">The total length to copy (starting from 0)</param>
        public void CopyTo(IntPtr ptr, int destinationIndex, int length)
        {
            if (ptr == null)
                throw new ArgumentNullException(nameof(ptr));

            if ((uint)destinationIndex > (uint)length)
                throw new ArgumentOutOfRangeException("Length must be smaller than DestinationIndex");

            Memory.CopyMemory(new IntPtr(m_buffer), 0, ptr, destinationIndex, Math.Min(length, BytesUsed));
        }


        /// <summary>
        /// Copies the inner buffer to a supplied buffer.
        /// </summary>
        /// <param name="buffer">The destination for the data.</param>
        public void CopyTo(void* ptr)
        {
            CopyTo(ptr, 0, BytesUsed);
        }

        /// <summary>
        /// Copies the inner buffer to a supplied buffer.
        /// </summary>
        /// <param name="buffer">The destination for the data.</param>
        public void CopyTo(void* ptr, int destinationIndex)
        {
            CopyTo(ptr, destinationIndex, BytesUsed);
        }

        /// <summary>
        /// Copies the inner buffer to a supplied buffer.
        /// </summary>
        /// <param name="buffer">The destination for the data.</param>
        /// <param name="length">The total length to copy (starting from 0)</param>
        public void CopyTo(void* ptr, int destinationIndex, int length)
        {
            if (ptr == null)
                throw new ArgumentNullException(nameof(ptr));

            if ((uint)destinationIndex > (uint)length)
                throw new ArgumentOutOfRangeException("Length must be smaller than DestinationIndex");

            Memory.CopyMemory(m_buffer, 0, ptr, destinationIndex, Math.Min(length, BytesUsed));
        }
    }
}
