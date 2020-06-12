/*
 *  Copyright (c) 2019 Dennis Corvers
 *
 *  Permission is hereby granted, free of charge, to any person obtaining a copy
 *  of this software and associated documentation files (the "Software"), to deal
 *  in the Software without restriction, including without limitation the rights
 *  to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
 *  copies of the Software, and to permit persons to whom the Software is
 *  furnished to do so, subject to the following conditions:
 *
 *  The above copyright notice and this permission notice shall be included in all
 *  copies or substantial portions of the Software.
 *
 *  THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 *  IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 *  FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 *  AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 *  LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 *  OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
 *  SOFTWARE.
 */
using BitSerializer.Utils;
using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace BitSerializer
{
    public unsafe partial class BitStream : IDisposable
    {
        public const int DefaultSize = 1200;

#pragma warning disable IDE0032
        private ulong* m_buffer;
        private int m_bitLength;

        private int m_offset;
        private SerializationMode m_mode;
        private bool m_ownsBuffer;

#pragma warning restore

        public BitStream() { }
        ~BitStream()
        {
            if (m_ownsBuffer)
                DeallocateInnerBuffer();
        }

        /// <summary>
        /// The current stream offset in bits.
        /// </summary>
        public int BitOffset
        { get { return m_offset; } }
        /// <summary>
        /// The current stream offset in bytes.
        /// </summary>
        public double ByteOffset
            => (double)BitOffset / 8;
        /// <summary>
        /// The amount of bytes occupied by the stream.
        /// </summary>
        public int BytesUsed
            => (m_offset + 8 - 1) / 8;
        /// <summary>
        /// The total stream length in bits.
        /// </summary>
        public int BitLength
            => m_bitLength;
        /// <summary>
        /// The total stream length in bytes.
        /// </summary>
        public int ByteLength
            => m_bitLength >> 3;

        /// <summary>
        /// The current streaming mode.
        /// </summary>
        public SerializationMode Mode
            => m_mode;
        /// <summary>
        /// Determines if the stream is writing.
        /// </summary>
        public bool IsWriting
            => m_mode == SerializationMode.Writing;
        /// <summary>
        /// Determines if the stream is reading.
        /// </summary>
        public bool IsReading
            => m_mode == SerializationMode.Reading;
        /// <summary>
        /// The inner buffer used by the stream.
        /// </summary>
        public IntPtr Buffer
            => (IntPtr)m_buffer;
        /// <summary>
        /// TRUE if this Bitstream has allocated its own buffer.
        /// </summary>
        public bool OwnsBuffer
            => m_ownsBuffer;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void EnsureSize(int bitCount)
        {
            Debug.Assert(bitCount > 0, "Amount of bits must be larger than zero.");
            Debug.Assert(bitCount <= 64, "Amount of bits may not be larger than 64.");

            if (m_offset + bitCount > m_bitLength)
            { throw new InvalidOperationException("Inner buffer is exceeded"); }
        }

        /// <summary>
        /// Resets stream for reading (reads what was written so far).
        /// </summary>
        public void ResetRead()
        {
            m_mode = SerializationMode.Reading;
            m_offset = 0;
        }

        /// <summary>
        /// Resets stream for reading and copies data.
        /// Allocates a new inner buffer!
        /// </summary>
        public void ResetRead(byte[] data)
        {
            ResetRead(data, 0, data.Length);
        }

        /// <summary>
        /// Copies the buffer for reading.
        /// </summary>
        public void ResetRead(byte[] data, int offset, int length)
        {
            Debug.Assert(offset >= 0, "Offset must be greater than zero.");
            if (length + offset > data.Length) { throw new ArgumentOutOfRangeException("length"); }

            fixed (byte* ptr = &data[offset])
            { ResetRead((IntPtr)ptr, length); }
        }

        /// <summary>
        /// Copies the buffer for reading.
        /// </summary>
        /// <param name="buffer">The buffer to read from</param>
        /// <param name="length">The length of the buffer in bytes</param>
        /// <param name="copy">True to allocate and copy to the inner buffer</param>
        public void ResetRead(IntPtr buffer, int length, bool copy = true)
        {
            ResetRead(buffer, length, 0, copy);
        }
        /// <summary>
        /// Copies the buffer for reading.
        /// </summary>
        /// <param name="buffer">The buffer to read from</param>
        /// <param name="length">The length of the buffer in bytes</param>
        /// <param name="offset">The offset where to start reading</param>
        /// <param name="copy">True to allocate and copy to the inner buffer</param>
        public void ResetRead(IntPtr buffer, int length, int offset, bool copy = true)
        {
            Debug.Assert(length > offset, "Offset must not exceed length.");
            if (copy)
            {
                if (buffer == IntPtr.Zero)
                    throw new ArgumentNullException("buffer");

                AllocateBuffer(MathUtils.GetNextMultiple(length, 8));
                Memory.CopyMemory((void*)buffer, 0, m_buffer, 0, length);
            }
            else
            {
                if (length < 8)
                    throw new ArgumentOutOfRangeException("Length must be at least 8 bytes.");

                m_buffer = (ulong*)buffer;
                m_bitLength = length * 8;
            }
            m_offset = 0;
            m_mode = SerializationMode.Reading;
        }

        /// <summary>
        /// Resets the stream for writing,
        /// Allocates a new buffer is none is yet allocated.
        /// </summary>
        public void ResetWrite()
        {
            if (m_buffer == null)
            { ResetWrite(DefaultSize); return; }
            else
            {
                m_offset = 0;
                m_mode = SerializationMode.Writing;
            }
        }
        /// <summary>
        /// Resets the stream for writing.
        /// Allocates a new buffer!
        /// </summary>
        /// <param name="length">The length of the buffer to allocate in bytes.</param>
        public void ResetWrite(int length)
        {
            if (length < 1)
                throw new ArgumentOutOfRangeException("Length must be greater than zero.");

            AllocateBuffer(MathUtils.GetNextMultiple(length, 8));
            m_offset = 0;
            m_mode = SerializationMode.Writing;
        }
        /// <summary>
        /// Resets the stream for writing using an existing buffer.
        /// </summary>
        /// <param name="buffer">The buffer to write to</param>
        /// <param name="length">The length of the supplied buffer</param>
        /// <param name="copy">Determines if a copy should be made</param>
        public void ResetWrite(IntPtr buffer, int length, bool copy = true)
        {
            ResetWrite(buffer, length, 0, copy);
        }
        /// <summary>
        /// Resets the stream for writing using an existing buffer.
        /// </summary>
        /// <param name="buffer">The buffer to write to</param>
        /// <param name="length">The length of the supplied buffer</param>
        /// <param name="offset">The offset where to start writing</param>
        /// <param name="copy">Determines if a copy should be made</param>
        public void ResetWrite(IntPtr buffer, int length, int offset, bool copy = true)
        {
            Debug.Assert(length > offset, "Offset must not exceed length.");
            if (copy)
            {
                if (buffer == IntPtr.Zero)
                    throw new ArgumentNullException("buffer");

                AllocateBuffer(MathUtils.GetNextMultiple(length, 8));
                Memory.CopyMemory((void*)buffer, 0, m_buffer, 0, length);
            }
            else
            {
                if (length < 8)
                    throw new ArgumentOutOfRangeException("Length must be at least 8 bytes.");

                m_buffer = (ulong*)buffer;
                m_mode = SerializationMode.Writing;
                m_offset = 0;
                m_bitLength = length * 8;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void AllocateBuffer(int length)
        {
            Debug.Assert(length > 0, "Length must be at least 1.");

            int bitLength = length << 3;
            if (m_buffer == null)
            {
                m_buffer = (ulong*)Memory.Alloc(length);
                m_bitLength = bitLength;
            }
            else if (bitLength > m_bitLength)
            {
                m_buffer = (ulong*)Memory.Realloc((IntPtr)m_buffer, length);
                m_bitLength = bitLength;
            }

            m_ownsBuffer = true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public BitStream Serialize(ref double value)
        {
            if (m_mode == SerializationMode.Writing) WriteDouble(value);
            else value = ReadDouble();
            return this;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public BitStream Serialize(ref float value, bool halfPrecision = false)
        {
            if (!halfPrecision)
            {
                if (m_mode == SerializationMode.Writing) WriteFloat(value);
                else value = ReadFloat();
            }
            else
            {
                if (m_mode == SerializationMode.Writing) WriteHalf(value);
                else value = ReadHalf();
            }
            return this;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public BitStream Serialize(ref decimal value)
        {
            if (m_mode == SerializationMode.Writing) WriteDecimal(value);
            else value = ReadDecimal();
            return this;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public BitStream Serialize(ref bool value)
        {
            if (m_mode == SerializationMode.Writing) WriteBool(value);
            else value = ReadBool();
            return this;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public BitStream Serialize(ref sbyte value, int bitCount = 8)
        {
            if (m_mode == SerializationMode.Writing) WriteSByte(value, bitCount);
            else value = ReadSByte(bitCount);
            return this;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public BitStream Serialize(ref byte value, int bitCount = 8)
        {
            if (m_mode == SerializationMode.Writing) WriteByte(value, bitCount);
            else value = ReadByte(bitCount);
            return this;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public BitStream Serialize(ref short value, int bitCount = 16)
        {
            if (m_mode == SerializationMode.Writing) WriteShort(value, bitCount);
            else value = ReadShort(bitCount);
            return this;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public BitStream Serialize(ref ushort value, int bitCount = 16)
        {
            if (m_mode == SerializationMode.Writing) WriteUShort(value, bitCount);
            else value = ReadUShort(bitCount);
            return this;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public BitStream Serialize(ref int value, int bitCount = 32)
        {
            if (m_mode == SerializationMode.Writing) WriteInt32(value, bitCount);
            else value = ReadInt32(bitCount);
            return this;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public BitStream Serialize(ref uint value, int bitCount = 32)
        {
            if (m_mode == SerializationMode.Writing) WriteUInt32(value, bitCount);
            else value = ReadUInt32(bitCount);
            return this;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public BitStream Serialize(ref long value, int bitCount = 64)
        {
            if (m_mode == SerializationMode.Writing) WriteLong(value, bitCount);
            else value = ReadLong(bitCount);
            return this;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public BitStream Serialize(ref ulong value, int bitCount = 64)
        {
            if (m_mode == SerializationMode.Writing) WriteULong(value, bitCount);
            else value = ReadULong(bitCount);
            return this;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public BitStream Serialize(ref char value, int bitCount = 16)
        {
            if (m_mode == SerializationMode.Writing) WriteChar(value, bitCount);
            else value = ReadChar(bitCount);
            return this;
        }

        public void Dispose()
        {
            DeallocateInnerBuffer();
        }

        /// <summary>
        /// Skips a certain number of bits by writing 0's
        /// </summary>
        /// <param name="bitCount">Amount of bits to skip</param>
        public BitStream Zeroes(int bitCount)
        {
            Debug.Assert(bitCount > 0, "Amount of bits must be larger than zero.");

            if (m_mode == SerializationMode.Writing)
            {
                if (m_offset + bitCount > m_bitLength)
                    throw new InvalidOperationException("Inner buffer is exceeded");

                int numLongs = bitCount / 64;

                for (int i = 0; i < numLongs; i++)
                { Write(0, 64); }

                Write(0, bitCount);
            }
            else
            {
                m_offset += bitCount;
            }
            return this;
        }

        /// <summary>
        /// Deallocates the buffer currently in use.
        /// Uses Memory.Dealloc
        /// </summary>
        public void DeallocateInnerBuffer()
        {
            if ((IntPtr)m_buffer == IntPtr.Zero) { return; }
            Memory.Dealloc(m_buffer);
            m_offset = 0;
            m_bitLength = 0;
            m_mode = SerializationMode.None;
            m_buffer = null;
            m_ownsBuffer = false;
        }

        #region Reading
        private ulong Read(int bits)
        {
            ulong value = InternalPeek(bits);
            m_offset += bits;
            return value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private ulong InternalPeek(int bits)
        {
            Debug.Assert(m_mode == SerializationMode.Reading, "Buffer is not in read mode. Call ResetRead first.");
            EnsureSize(bits);

            int longOffsetStart = m_offset >> 6;
            int longOffsetEnd = (m_offset + bits - 1) >> 6;

            ulong basemask = ulong.MaxValue >> (64 - bits);
            int placeOffset = m_offset & 0x3F;

            ulong value = m_buffer[longOffsetStart] >> placeOffset;

            if (longOffsetEnd != longOffsetStart)
            { value |= m_buffer[longOffsetEnd] << (64 - placeOffset); }

            return value & basemask;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public double ReadDouble()
        {
            ulong val = Read(64);
            return *(double*)&val;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public float ReadFloat()
        {
            uint val = unchecked((uint)Read(32));
            return *(float*)&val;

        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public decimal ReadDecimal()
        {
            decimal result;
            ((ulong*)&result)[0] = Read(sizeof(ulong) * 8);
            ((ulong*)&result)[1] = Read(sizeof(ulong) * 8);
            return result;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool ReadBool()
        {
            return Read(1) == 1;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public sbyte ReadSByte(int bitCount = 8)
        {
            return (sbyte)ZigZag.Zag(ReadUInt32(bitCount));
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public short ReadShort(int bitCount = 16)
        {
            return (short)ZigZag.Zag(ReadUInt32(bitCount));
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int ReadInt32(int bitCount = 32)
        {
            return ZigZag.Zag(ReadUInt32(bitCount));
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public long ReadLong(int bitCount = 64)
        {
            return ZigZag.Zag(ReadULong(bitCount));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public byte ReadByte(int bitCount = 8)
        {
            return unchecked((byte)Read(bitCount));
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ushort ReadUShort(int bitCount = 16)
        {
            return unchecked((ushort)Read(bitCount));
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public uint ReadUInt32(int bitCount = 32)
        {
            return unchecked((uint)Read(bitCount));
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ulong ReadULong(int bitCount = 64)
        {
            return unchecked(Read(bitCount));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public char ReadChar(int bitCount = 16)
        {
            return unchecked((char)Read(bitCount));
        }


        #endregion

        #region Peek

        private ulong Peek(int bits)
        {
            return InternalPeek(bits);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public double PeekDouble()
        {
            ulong val = Peek(64);
            return *(double*)&val;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public float PeekFloat()
        {
            ulong val = Peek(32);
            return *(float*)&val;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public decimal PeekDecimal()
        {
            decimal result;
            ((ulong*)&result)[0] = Peek(sizeof(ulong) * 8);
            ((ulong*)&result)[1] = Peek(sizeof(ulong) * 8);
            return result;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool PeekBool()
        {
            return Peek(1) == 1;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public sbyte PeekSByte(int bitCount = 8)
        {
            return (sbyte)ZigZag.Zag(PeekUInt32(bitCount));
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public short PeekShort(int bitCount = 16)
        {
            return (short)ZigZag.Zag(PeekUInt32(bitCount));
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int PeekInt32(int bitCount = 32)
        {
            return ZigZag.Zag(PeekUInt32(bitCount));
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public long PeekLong(int bitCount = 64)
        {
            return ZigZag.Zag(PeekULong(bitCount));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public byte PeekByte(int bitCount = 8)
        {
            return unchecked((byte)Peek(bitCount));
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ushort PeekUShort(int bitCount = 16)
        {
            return unchecked((ushort)Peek(bitCount));
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public uint PeekUInt32(int bitCount = 32)
        {
            return unchecked((uint)Peek(bitCount));
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ulong PeekULong(int bitCount = 64)
        {
            return unchecked(Peek(bitCount));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public char PeekChar(int bitCount = 16)
        {
            return unchecked((char)Peek(bitCount));
        }

        #endregion

        #region Writing
        private void Write(ulong value, int bits)
        {
            Debug.Assert(m_mode == SerializationMode.Writing, "Buffer is not in write mode. Call ResetWrite first.");
            EnsureSize(bits);

            int longOffsetStart = m_offset >> 6;
            int longOffsetEnd = (m_offset + bits - 1) >> 6;

            ulong basemask = ulong.MaxValue >> (64 - bits);
            int placeOffset = m_offset & 0x3F;

            value = value & basemask;

            if (placeOffset == 0)
            { m_buffer[longOffsetStart] = value << placeOffset; }
            else
            { m_buffer[longOffsetStart] |= value << placeOffset; }

            if (longOffsetEnd != longOffsetStart)
            { m_buffer[longOffsetEnd] = value >> (64 - placeOffset); }
            m_offset += bits;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public BitStream WriteDouble(double value)
        {
            Write(*(ulong*)&value, sizeof(double) * 8);
            return this;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public BitStream WriteFloat(float value)
        {
            Write(*(uint*)&value, sizeof(float) * 8);
            return this;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public BitStream WriteDecimal(decimal value)
        {
            Write(((ulong*)&value)[0], sizeof(ulong) * 8);
            Write(((ulong*)&value)[1], sizeof(ulong) * 8);
            return this;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public BitStream WriteBool(bool value)
        {
            Write(value ? (ulong)1 : 0, 1);
            return this;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public BitStream WriteSByte(sbyte value, int bitCount = 8)
        {
            WriteULong(ZigZag.Zig(value), bitCount);
            return this;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public BitStream WriteShort(short value, int bitCount = 16)
        {
            WriteULong(ZigZag.Zig(value), bitCount);
            return this;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public BitStream WriteInt32(int value, int bitCount = 32)
        {
            WriteULong(ZigZag.Zig(value), bitCount);
            return this;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public BitStream WriteLong(long value, int bitCount = 64)
        {
            WriteULong(ZigZag.Zig(value), bitCount);
            return this;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public BitStream WriteByte(byte value, int bitCount = 8)
        {
            Write(value, bitCount);
            return this;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public BitStream WriteUShort(ushort value, int bitCount = 16)
        {
            Write(value, bitCount);
            return this;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public BitStream WriteUInt32(uint value, int bitCount = 32)
        {
            Write(value, bitCount);
            return this;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public BitStream WriteULong(ulong value, int bitCount = 64)
        {
            Write(value, bitCount);
            return this;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public BitStream WriteChar(char value, int bitCount = 16)
        {
            Write(value, bitCount);
            return this;
        }

        #endregion
    }
}
