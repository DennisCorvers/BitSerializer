using BitSerializer.Utils;
using System;
using System.Diagnostics;

namespace BitSerializer
{
    public unsafe partial class BitStream
    {
        public void WriteMemory(IntPtr ptr, int byteSize)
        { WriteMemory((void*)ptr, byteSize); }
        public void WriteMemory(void* ptr, int byteSize)
        {
            int numLongs = byteSize / 8;
            ulong* p = (ulong*)ptr;
            for (int i = 0; i < numLongs; i++)
            { Write(p[i], 64); }

            byte* bptr = (byte*)&p[numLongs];
            for (int i = byteSize - numLongs * 8; i > 0; i--)
            {
                WriteByte(*bptr);
                bptr++;
            }
        }

        public void ReadMemory(IntPtr ptr, int byteSize)
        { ReadMemory((void*)ptr, byteSize); }
        public void ReadMemory(void* ptr, int byteSize)
        {
            int numLongs = byteSize / 8;
            ulong* p = (ulong*)ptr;
            for (int i = 0; i < numLongs; i++)
            { p[i] = Read(64); }

            byte* bptr = (byte*)&p[numLongs];
            for (int i = byteSize - numLongs * sizeof(ulong); i > 0; i--)
            {
                *bptr = ReadByte(); bptr++;
            }
        }

        public void WriteBytes(byte[] bytes, int offset, int count, bool includeSize = false)
        {
            Debug.Assert(bytes.Length >= offset + count, "Offset and count exceed array size");
            Debug.Assert(offset >= 0, "Offset must be at least 0");
            Debug.Assert(count > 0, "Count must be at least 1");

            if (includeSize)
            { WriteUShort((ushort)count); }

            fixed (byte* ptr = &bytes[offset])
            { WriteMemory(ptr, count); }
        }
        public void ReadBytes(byte[] bytes, int offset, int count)
        {
            Debug.Assert(bytes.Length >= offset + count, "Offset and count exceed array size");
            Debug.Assert(offset >= 0, "Offset must be at least 0");
            Debug.Assert(count > 0, "Count must be at least 1");

            fixed (byte* ptr = &bytes[offset])
            { ReadMemory(ptr, count); ; }
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
        { CopyTo(buffer, 0, BytesUsed); }
        /// <summary>
        /// Copies the inner buffer to a supplied buffer.
        /// </summary>
        /// <param name="buffer">The destination for the data.</param>
        public void CopyTo(byte[] buffer, int destinationIndex)
        { CopyTo(buffer, destinationIndex, BytesUsed); }
        /// <summary>
        /// Copies the inner buffer to a supplied buffer.
        /// </summary>
        /// <param name="buffer">The destination for the data.</param>
        /// <param name="length">The total length to copy (starting from 0)</param>
        public void CopyTo(byte[] buffer, int destinationIndex, int length)
        {
            Debug.Assert(buffer != null, "ArgumentNull buffer");
            Debug.Assert(length <= BytesUsed, "Length exceeds BitStream buffer size.");

            Memory.CopyMemory(new IntPtr(m_buffer), 0, buffer, destinationIndex, length);
        }

        /// <summary>
        /// Copies the inner buffer to a supplied buffer.
        /// </summary>
        /// <param name="buffer">The destination for the data.</param>
        public void CopyTo(IntPtr ptr)
        { CopyTo(ptr, 0, BytesUsed); }
        /// <summary>
        /// Copies the inner buffer to a supplied buffer.
        /// </summary>
        /// <param name="buffer">The destination for the data.</param>
        public void CopyTo(IntPtr ptr, int destinationIndex)
        { CopyTo(ptr, destinationIndex, BytesUsed); }
        /// <summary>
        /// Copies the inner buffer to a supplied buffer.
        /// </summary>
        /// <param name="buffer">The destination for the data.</param>
        /// <param name="length">The total length to copy (starting from 0)</param>
        public void CopyTo(IntPtr ptr, int destinationIndex, int length)
        {
            Debug.Assert(ptr != IntPtr.Zero, "ArgumentNull, bufffer.");
            Debug.Assert(length <= BytesUsed, "Length exceeds BitStream buffer size.");

            Memory.CopyMemory(new IntPtr(m_buffer), 0, ptr, destinationIndex, length);
        }

        /// <summary>
        /// Copies the inner buffer to a supplied buffer.
        /// </summary>
        /// <param name="buffer">The destination for the data.</param>
        public void CopyTo(void* ptr)
        { CopyTo(ptr, 0, BytesUsed); }
        /// <summary>
        /// Copies the inner buffer to a supplied buffer.
        /// </summary>
        /// <param name="buffer">The destination for the data.</param>
        public void CopyTo(void* ptr, int destinationIndex)
        { CopyTo(ptr, destinationIndex, BytesUsed); }
        /// <summary>
        /// Copies the inner buffer to a supplied buffer.
        /// </summary>
        /// <param name="buffer">The destination for the data.</param>
        /// <param name="length">The total length to copy (starting from 0)</param>
        public void CopyTo(void* ptr, int destinationIndex, int length)
        {
            Debug.Assert(ptr != null, "ArgumentNull, bufffer.");
            Debug.Assert(length <= BytesUsed, "Length exceeds BitStream buffer size.");

            Memory.CopyMemory(m_buffer, 0, ptr, destinationIndex, length);
        }
    }
}
