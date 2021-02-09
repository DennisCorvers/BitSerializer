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
        public const int MinSize = 16;

#pragma warning disable IDE0032
        private ulong* m_buffer;
        private int m_bitLength;

        private int m_offset;
        private SerializationMode m_mode;
        private bool m_ownsBuffer;
#pragma warning restore

        public BitStream()
        { }

        ~BitStream()
        {
            if (m_ownsBuffer)
                DeallocateInnerBuffer();
        }

        /// <summary>
        /// The current <see cref="BitStream"/> offset in bits.
        /// </summary>
        public int BitOffset
            => m_offset;
        /// <summary>
        /// The current <see cref="BitStream"/> offset in bytes.
        /// </summary>
        public double ByteOffset
            => (double)BitOffset / 8;
        /// <summary>
        /// The amount of bytes occupied by the <see cref="BitStream"/>.
        /// </summary>
        public int BytesUsed
            => (m_offset + 7) >> 3;
        /// <summary>
        /// The total <see cref="BitStream"/> length in bits.
        /// </summary>
        public int BitLength
            => m_bitLength;
        /// <summary>
        /// The total <see cref="BitStream"/> length in bytes.
        /// </summary>
        public int ByteLength
            => m_bitLength >> 3;

        /// <summary>
        /// The current streaming mode.
        /// </summary>
        public SerializationMode Mode
            => m_mode;
        /// <summary>
        /// Determines if the <see cref="BitStream"/> is writing.
        /// </summary>
        public bool IsWriting
            => m_mode == SerializationMode.Writing;
        /// <summary>
        /// Determines if the <see cref="BitStream"/> is reading.
        /// </summary>
        public bool IsReading
            => m_mode == SerializationMode.Reading;
        /// <summary>
        /// The inner buffer used by the <see cref="BitStream"/>.
        /// </summary>
        public IntPtr Buffer
            => (IntPtr)m_buffer;
        /// <summary>
        /// TRUE if this <see cref="BitStream"/> has allocated its own buffer.
        /// </summary>
        public bool OwnsBuffer
            => m_ownsBuffer;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void EnsureReadSize(int bitCount)
        {
            Debug.Assert(bitCount > 0, "Amount of bits must be larger than zero.");
            Debug.Assert(m_mode == SerializationMode.Reading);

            // Casting to uint also checks negative bitCount values.
            if (m_offset + (uint)bitCount > m_bitLength)
                throw new InvalidOperationException("Inner buffer is exceeded");
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void EnsureWriteBits(int bitCount)
        {
            Debug.Assert(bitCount > 0, "Amount of bits must be larger than zero.");
            Debug.Assert(m_mode == SerializationMode.Writing);

            // Casting to uint checks negative numbers.
            long newSize = m_offset + (uint)bitCount;

            if (newSize > m_bitLength)
                Resize((int)newSize);
        }

        private void Resize(int bufferBitSize)
        {
            if (!m_ownsBuffer)
                throw new InvalidOperationException("Inner buffer is exceeded");

            int newBitSize = Math.Max(m_bitLength * 2, bufferBitSize);
            int newByteSize = MathUtils.GetNextMultipleOf8(newBitSize >> 3);

            m_buffer = (ulong*)Memory.Realloc((IntPtr)m_buffer, m_bitLength >> 3, newByteSize);
            m_bitLength = newByteSize * 8;
        }

        /// <summary>
        /// Resets <see cref="BitStream"/> for reading (reads what was written so far).
        /// </summary>
        public void ResetRead()
        {
            if (m_buffer == null)
                throw new InvalidOperationException("BitStream has no valid buffer.");

            m_mode = SerializationMode.Reading;
            m_offset = 0;
        }

        /// <summary>
        /// Resets <see cref="BitStream"/> for reading and copies data.
        /// Allocates a new inner buffer!
        /// </summary>
        public void ResetRead(byte[] data)
        {
            ResetRead(data, 0, data.Length);
        }

        /// <summary>
        /// Copies the buffer for reading.
        /// </summary>
        /// <param name="data">The buffer to read from</param>
        /// <param name="length">The length of the buffer in bytes</param>
        /// <param name="offset">The offset where to start reading</param>
        public void ResetRead(byte[] data, int offset, int length)
        {
            if (data == null)
                throw new ArgumentNullException(nameof(data));

            if ((uint)offset + (uint)length > data.Length)
                throw new ArgumentOutOfRangeException("Offset must be smaller than array size.");

            fixed (byte* ptr = &data[offset])
            {
                ResetRead((IntPtr)ptr, length, true);
            }
        }

        /// <summary>
        /// Uses the supplied buffer for reading.
        /// </summary>
        /// <param name="buffer">The buffer to read from</param>
        /// <param name="length">The length of the buffer in bytes</param>
        public void ResetRead(IntPtr buffer, int length)
        {
            ResetRead(buffer, length, false);
        }

        /// <summary>
        /// Copies the buffer for reading.
        /// </summary>
        /// <param name="buffer">The buffer to read from</param>
        /// <param name="length">The length of the buffer in bytes</param>
        /// <param name="copy">True to allocate and copy to the inner buffer</param>
        public void ResetRead(IntPtr buffer, int length, bool copy)
        {
            var ptr = (void*)buffer;
            if (ptr == null)
                throw new ArgumentNullException(nameof(buffer));

            if (copy)
                CopyBufferToInternal(ptr, length);
            else
                SetBufferAsInternal(ptr, length);

            m_offset = 0;
            m_mode = SerializationMode.Reading;
        }

        /// <summary>
        /// Resets the <see cref="BitStream"/> for writing,
        /// Allocates a new buffer is none is yet allocated.
        /// </summary>
        public void ResetWrite()
        {
            if (m_buffer == null)
            {
                ResetWrite(DefaultSize);
                return;
            }
            else
            {
                // Clear existing memory.
                Memory.ZeroMem(m_buffer, m_bitLength >> 3);

                m_offset = 0;
                m_mode = SerializationMode.Writing;
            }
        }

        /// <summary>
        /// Resets the <see cref="BitStream"/> for writing.
        /// Allocates a new buffer!
        /// </summary>
        /// <param name="length">The length of the buffer to allocate in bytes.</param>
        public void ResetWrite(int length)
        {
            if (length < 1)
                throw new ArgumentOutOfRangeException(nameof(length), "Length must be greater than zero.");

            AllocateBuffer(MathUtils.GetNextMultipleOf8(length));
            m_offset = 0;
            m_mode = SerializationMode.Writing;
        }

        /// <summary>
        /// Resets the <see cref="BitStream"/> for writing using an existing buffer.
        /// </summary>
        /// <param name="buffer">The buffer to write to</param>
        /// <param name="length">The length of the supplied buffer</param>
        public void ResetWrite(IntPtr buffer, int length)
        {
            ResetWrite(buffer, length, false);
        }

        /// <summary>
        /// Resets the <see cref="BitStream"/> for writing using an existing buffer.
        /// </summary>
        /// <param name="buffer">The buffer to write to</param>
        /// <param name="length">The length of the supplied buffer</param>
        /// <param name="copy">Appends the supplied buffer at the front of the<see cref="BitStream"/>.</param>
        public void ResetWrite(IntPtr buffer, int length, bool copy)
        {
            var ptr = (void*)buffer;
            if (ptr == null)
                throw new ArgumentNullException(nameof(buffer));

            if (copy)
            {
                CopyBufferToInternal(ptr, length);
                m_offset = length * 8;
            }
            else
            {
                SetBufferAsInternal(ptr, length);

                // Clear out buffer and set the offset to zero.
                Memory.ZeroMem(m_buffer, m_bitLength >> 3);
                m_offset = 0;
            }

            m_mode = SerializationMode.Writing;
        }

        private void CopyBufferToInternal(void* buffer, int length)
        {
            int bitLength = length << 3;
            int bytesToClear = ByteLength - length;
            if (m_ownsBuffer && m_bitLength < bitLength)
            {
                // We need to reallocate the buffer because it is too small.
                // Free the buffer first to we can allocate it later and copy.
                Memory.Free(m_buffer);
                m_buffer = null;
                m_ownsBuffer = false;
            }

            if (!m_ownsBuffer)
            {
                // Allocate a new buffer.
                int newByteLength = Math.Max(m_bitLength >> 3, MathUtils.GetNextMultipleOf8(length));
                m_buffer = (ulong*)Memory.Alloc(newByteLength);
                m_bitLength = newByteLength * 8;
                bytesToClear = newByteLength - length;
            }
            // Copy the buffer
            Memory.CopyMemory(buffer, m_buffer, length);

            // Zero any remaining bytes.
            Memory.ZeroMem((byte*)m_buffer + length, bytesToClear);

            m_ownsBuffer = true;
        }

        private void SetBufferAsInternal(void* buffer, int length)
        {
            if (m_ownsBuffer)
                throw new InvalidOperationException("BitStream owns a buffer. Call Deallocate first.");

            // Round down to previous multiple of 8 so we don't overwrite
            // memory that is located beyond out buffer.
            length = MathUtils.GetPreviousMultipleOf8(length);

            if (length < 8)
                throw new ArgumentOutOfRangeException(nameof(length), "Length must be at least 8 bytes.");

            m_buffer = (ulong*)buffer;
            m_bitLength = length * 8;
        }

        /// <summary>
        /// Reserves 4 bytes at the front of the <see cref="BitStream"/> so that the size can be written later.
        /// </summary>
        public void ReserveSizePrefix()
        {
            Zeroes(32);
        }

        /// <summary>
        /// Prefixes the <see cref="BitStream"/> with the total size of this <see cref="BitStream"/>.
        /// Overwrites any data that might be in the first 32 bits.
        /// </summary>
        public int PrefixSize()
        {
            if (m_mode != SerializationMode.Writing)
                throw new InvalidOperationException("Buffer is not in write mode.");

            *(int*)m_buffer = BytesUsed;
            return BytesUsed;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void AllocateBuffer(int length)
        {
            Debug.Assert(length > 0);
            Debug.Assert(length % 8 == 0);

            int bitLength = length << 3;
            if (m_ownsBuffer && m_bitLength < bitLength)
            {
                Memory.Free(m_buffer);
                m_ownsBuffer = false;
            }

            if (!m_ownsBuffer)
            {
                int newByteLength = Math.Max(m_bitLength >> 3, length);
                m_buffer = (ulong*)Memory.Alloc(newByteLength);
                m_bitLength = newByteLength * 8;
            }

            Memory.ZeroMem((byte*)m_buffer, m_bitLength >> 3);
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
        /// Skips a certain number of bits. Writes 0 bits when in write-mode.
        /// </summary>
        /// <param name="bitCount">Amount of bits to skip</param>
        public BitStream Zeroes(int bitCount)
        {
            if (bitCount < 0)
                throw new ArgumentOutOfRangeException(nameof(bitCount));

            if (m_mode == SerializationMode.Writing)
            {
                EnsureWriteBits(bitCount);

                // Write the long values first.
                while (bitCount > 64)
                {
                    Write(0, 64);
                    bitCount -= 64;
                }

                // Write the remaining bits.
                if (bitCount > 0)
                    Write(0, bitCount);
            }
            else
            {
                EnsureReadSize(bitCount);
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
            if (!m_ownsBuffer)
                return;

            Memory.Free(m_buffer);
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
            EnsureReadSize(bits);

            int longOffsetStart = m_offset >> 6;
            int longOffsetEnd = (m_offset + bits - 1) >> 6;

            ulong basemask = ulong.MaxValue >> (64 - bits);
            int placeOffset = m_offset & 0x3F;

            ulong value = m_buffer[longOffsetStart] >> placeOffset;

            if (longOffsetEnd != longOffsetStart)
            {
                value |= m_buffer[longOffsetEnd] << (64 - placeOffset);
            }

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
            EnsureWriteBits(bits);

            int longOffsetStart = m_offset >> 6;
            int longOffsetEnd = (m_offset + bits - 1) >> 6;

            ulong basemask = ulong.MaxValue >> (64 - bits);
            int placeOffset = m_offset & 0x3F;

            value = value & basemask;
            m_buffer[longOffsetStart] |= value << placeOffset;

            if (longOffsetEnd != longOffsetStart)
                m_buffer[longOffsetEnd] = value >> (64 - placeOffset);

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
            Write(ZigZag.Zig(value), bitCount);
            return this;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public BitStream WriteShort(short value, int bitCount = 16)
        {
            Write(ZigZag.Zig(value), bitCount);
            return this;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public BitStream WriteInt32(int value, int bitCount = 32)
        {
            Write(ZigZag.Zig(value), bitCount);
            return this;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public BitStream WriteLong(long value, int bitCount = 64)
        {
            Write(ZigZag.Zig(value), bitCount);
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
