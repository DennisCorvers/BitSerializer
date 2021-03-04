/*
 *  Copyright (c) 2021 Dennis Corvers
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
    public unsafe partial class BitStreamer : IDisposable
    {
        public const int DefaultSize = 1200;
        public const int MinSize = 16;

#pragma warning disable IDE0032
        private ulong* m_buffer;
        /// <summary>
        /// The entire bit size of the buffer.
        /// </summary>
        private int m_fullBitLength;
        /// <summary>
        /// Bit size of the buffer rounded down to the nearest multiple of 8.
        /// This is to prevent overwriting memory that isn't ours.
        /// </summary>
        private int m_bitWriteLength;

        private int m_offset;
        private SerializationMode m_mode;
        private bool m_ownsBuffer;
        private bool m_isValid;
        private bool m_shouldThrow;
#pragma warning restore

        /// <summary>
        /// Creates a new instance of <see cref="BitStreamer"/>
        /// </summary>
        public BitStreamer()
        {
            m_isValid = false;
            m_shouldThrow = true;
        }

        /// <summary>
        /// Creates a new instance of <see cref="BitStreamer"/>
        /// </summary>
        /// <param name="shouldThrow">When TRUE, exceeding the buffer throws an exception instead.</param>
        public BitStreamer(bool shouldThrow)
        {
            m_isValid = false;
            m_shouldThrow = shouldThrow;
        }

        ~BitStreamer()
        {
            if (m_ownsBuffer)
                DeallocateInnerBuffer();
        }

        /// <summary>
        /// The current <see cref="BitStreamer"/> offset in bits.
        /// </summary>
        public int BitOffset
            => m_offset;
        /// <summary>
        /// The current <see cref="BitStreamer"/> offset in bytes.
        /// </summary>
        public double ByteOffset
            => (double)BitOffset / 8;
        /// <summary>
        /// The amount of bytes occupied by the <see cref="BitStreamer"/>.
        /// </summary>
        public int BytesUsed
            => (m_offset + 7) >> 3;
        /// <summary>
        /// The total <see cref="BitStreamer"/> length in bits.
        /// </summary>
        public int BitLength
        {
            get
            {
                return IsReading ? m_fullBitLength : m_bitWriteLength;
            }
        }
        /// <summary>
        /// The total <see cref="BitStreamer"/> length in bytes.
        /// </summary>
        public int ByteLength
            => BitLength >> 3;

        /// <summary>
        /// The current streaming mode.
        /// </summary>
        public SerializationMode Mode
            => m_mode;
        /// <summary>
        /// Determines if the <see cref="BitStreamer"/> is writing.
        /// </summary>
        public bool IsWriting
            => m_mode == SerializationMode.Writing;
        /// <summary>
        /// Determines if the <see cref="BitStreamer"/> is reading.
        /// </summary>
        public bool IsReading
            => m_mode == SerializationMode.Reading;
        /// <summary>
        /// The inner buffer used by the <see cref="BitStreamer"/>.
        /// </summary>
        public IntPtr Buffer
            => (IntPtr)m_buffer;
        /// <summary>
        /// TRUE if this <see cref="BitStreamer"/> has allocated its own buffer.
        /// </summary>
        public bool OwnsBuffer
            => m_ownsBuffer;
        /// <summary>
        /// Determines if the <see cref="BitStreamer"/> throws an exception when the inner buffer bounds are exceeded.
        /// </summary>
        public bool ThrowsOnExceededBuffer
        {
            get => m_shouldThrow;
            set => m_shouldThrow = value;
        }
        /// <summary>
        /// Returns FALSE when the inner buffer bounds have been exceeded.
        /// </summary>
        public bool IsValid
            => m_isValid;


        /// <summary>
        /// Resets <see cref="BitStreamer"/> for reading (reads what was written so far).
        /// </summary>
        public void ResetRead()
        {
            if (m_buffer == null)
                throw new InvalidOperationException("BitStream has no valid buffer.");

            Reset(SerializationMode.Reading);
        }

        /// <summary>
        /// Resets <see cref="BitStreamer"/> for reading and copies data.
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
                SetBufferAsInternal(ptr, length, false);

            Reset(SerializationMode.Reading);
        }

        /// <summary>
        /// Resets the <see cref="BitStreamer"/> for writing,
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
                Memory.ZeroMem(m_buffer, m_fullBitLength >> 3);

                Reset(SerializationMode.Writing);
            }
        }

        /// <summary>
        /// Resets the <see cref="BitStreamer"/> for writing.
        /// Allocates a new buffer!
        /// </summary>
        /// <param name="length">The length of the buffer to allocate in bytes.</param>
        public void ResetWrite(int length)
        {
            if (length < 1)
                throw new ArgumentOutOfRangeException(nameof(length), "Length must be greater than zero.");

            AllocateBuffer(MathUtils.GetNextMultipleOf8(length));

            Reset(SerializationMode.Writing);
        }

        /// <summary>
        /// Resets the <see cref="BitStreamer"/> for writing using an existing buffer.
        /// Length will be rounded down to a multiple of 8.
        /// </summary>
        /// <param name="buffer">The buffer to write to</param>
        /// <param name="length">The length of the supplied buffer</param>
        public void ResetWrite(IntPtr buffer, int length)
        {
            ResetWrite(buffer, length, false);
        }

        /// <summary>
        /// Resets the <see cref="BitStreamer"/> for writing using an existing buffer.
        /// Length will be rounded down to a multiple of 8.
        /// </summary>
        /// <param name="buffer">The buffer to write to</param>
        /// <param name="length">The length of the supplied buffer</param>
        /// <param name="copy">Appends the supplied buffer at the front of the<see cref="BitStreamer"/>.</param>
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
                SetBufferAsInternal(ptr, length, true);

                // Clear out buffer and set the offset to zero.
                Memory.ZeroMem(m_buffer, m_fullBitLength >> 3);
                m_offset = 0;
            }

            m_mode = SerializationMode.Writing;
            m_isValid = true;
        }

        private void CopyBufferToInternal(void* buffer, int length)
        {
            int bitLength = length << 3;
            int bytesToClear = ByteLength - length;
            if (m_ownsBuffer && m_fullBitLength < bitLength)
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
                int newByteLength = Math.Max(m_fullBitLength >> 3, MathUtils.GetNextMultipleOf8(length));
                m_buffer = (ulong*)Memory.Alloc(newByteLength);
                m_bitWriteLength = m_fullBitLength = newByteLength * 8;
                bytesToClear = newByteLength - length;
            }
            // Copy the buffer
            Memory.CopyMemory(buffer, m_buffer, length);

            // Zero any remaining bytes.
            Memory.ZeroMem((byte*)m_buffer + length, bytesToClear);

            m_ownsBuffer = true;
        }

        private void SetBufferAsInternal(void* buffer, int length, bool writing)
        {
            if (m_ownsBuffer)
                throw new InvalidOperationException("BitStream owns a buffer. Call Deallocate first.");

            int rdLen = MathUtils.GetPreviousMultipleOf8(length);

            // Write buffer must have at least 8 bytes to write to.
            if (writing && rdLen < 8)
                throw new ArgumentOutOfRangeException(nameof(length), "Length must be at least 8 bytes.");

            m_buffer = (ulong*)buffer;
            m_fullBitLength = length * 8;
            m_bitWriteLength = rdLen * 8;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void Reset(SerializationMode mode)
        {
            m_offset = 0;
            m_mode = mode;
            m_isValid = true;
        }

        /// <summary>
        /// Reserves 4 bytes at the front of the <see cref="BitStreamer"/> so that the size can be written later.
        /// </summary>
        public void ReserveSizePrefix()
        {
            Skip(32);
        }

        /// <summary>
        /// Prefixes the <see cref="BitStreamer"/> with the total size of this <see cref="BitStreamer"/>.
        /// Overwrites any data that might be in the first 32 bits.
        /// </summary>
        public int PrefixSize()
        {
            if (m_mode != SerializationMode.Writing)
                throw new InvalidOperationException("Buffer is not in write mode.");

            *(int*)m_buffer = BytesUsed;
            return BytesUsed;
        }

        private void AllocateBuffer(int length)
        {
            Debug.Assert(length > 0);
            Debug.Assert(length % 8 == 0);

            int bitLength = length << 3;
            if (m_ownsBuffer && m_fullBitLength < bitLength)
            {
                Memory.Free(m_buffer);
                m_ownsBuffer = false;
            }

            if (!m_ownsBuffer)
            {
                int newByteLength = Math.Max(m_fullBitLength >> 3, length);
                m_buffer = (ulong*)Memory.Alloc(newByteLength);
                m_bitWriteLength = m_fullBitLength = newByteLength * 8;
            }

            Memory.ZeroMem((byte*)m_buffer, m_fullBitLength >> 3);
            m_ownsBuffer = true;
        }

        /// <summary>
        /// Skips a certain number of bits. Writes 0 bits when in write-mode.
        /// </summary>
        /// <param name="bitCount">Amount of bits to skip</param>
        public BitStreamer Skip(int bitCount)
        {
            if (bitCount <= 0)
                return this;

            if (m_mode == SerializationMode.Writing)
            {
                if (!EnsureWriteSize(bitCount))
                    return this;

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
                if (EnsureReadSize(bitCount))
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
            m_fullBitLength = 0;
            m_bitWriteLength = 0;
            m_mode = SerializationMode.None;
            m_buffer = null;
            m_ownsBuffer = false;
        }

        private ulong Read(int bits)
        {
            if (EnsureReadSize(bits))
            {
                ulong value = InternalPeek(bits);
                m_offset += bits;
                return value;
            }

            return 0;
        }

        /// <summary>
        /// Reads a value without ensuring the buffer size.
        /// </summary>
        private ulong ReadUnchecked(int bits)
        {
            ulong value = InternalPeek(bits);
            m_offset += bits;
            return value;
        }

        private ulong Peek(int bits)
        {
            if (EnsureReadSize(bits))
                return InternalPeek(bits);

            return 0;
        }

        private void Write(ulong value, int bits)
        {
            if (EnsureWriteSize(bits))
            {
                InternalWrite(value, bits);
                m_offset += bits;
            }
        }

        /// <summary>
        /// Writes a value without ensuring the buffer size.
        /// </summary>
        private void WriteUnchecked(ulong value, int bits)
        {
            InternalWrite(value, bits);
            m_offset += bits;
        }

        /// <summary>
        /// Reads a value without increasing the offset.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private ulong InternalPeek(int bits)
        {
            Debug.Assert(m_mode == SerializationMode.Reading, "Buffer is not in read mode. Call ResetRead first.");
            Debug.Assert(bits > 0);
            Debug.Assert(bits < 65);

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

        /// <summary>
        /// Writes a value without increasing the offset.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void InternalWrite(ulong value, int bits)
        {
            Debug.Assert(m_mode == SerializationMode.Writing, "Buffer is not in write mode. Call ResetWrite first.");
            Debug.Assert(bits > 0);
            Debug.Assert(bits < 65);

            int longOffsetStart = m_offset >> 6;
            int longOffsetEnd = (m_offset + bits - 1) >> 6;

            ulong basemask = ulong.MaxValue >> (64 - bits);
            int placeOffset = m_offset & 0x3F;

            value = value & basemask;
            m_buffer[longOffsetStart] |= value << placeOffset;

            if (longOffsetEnd != longOffsetStart)
                m_buffer[longOffsetEnd] = value >> (64 - placeOffset);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool EnsureReadSize(int bitCount)
        {
            Debug.Assert(bitCount > 0, "Amount of bits must be larger than zero.");
            Debug.Assert(m_mode == SerializationMode.Reading);

            if (m_offset + (uint)bitCount <= m_fullBitLength)
                return m_isValid = true;

            // Having the exception message in a different function improves performance
            // by ~15%, even if this branch is not used.
            ThrowException();

            return m_isValid = false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool EnsureWriteSize(int bitCount)
        {
            Debug.Assert(bitCount > 0, "Amount of bits must be larger than zero.");
            Debug.Assert(m_mode == SerializationMode.Writing);

            // Casting to uint checks negative numbers.
            long newSize = m_offset + (uint)bitCount;

            if (newSize > m_bitWriteLength)
                return m_isValid = Resize((int)newSize);

            return m_isValid = true;
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private void ThrowException()
        {
            if (m_shouldThrow)
                throw new InvalidOperationException("Inner buffer is exceeded.");
        }

        private bool Resize(int bufferBitSize)
        {
            if (!m_ownsBuffer)
            {
                ThrowException();
                return false;
            }

            int newBitSize = Math.Max(m_fullBitLength * 2, bufferBitSize);
            int newByteSize = MathUtils.GetNextMultipleOf8(newBitSize >> 3);

            m_buffer = (ulong*)Memory.Realloc((IntPtr)m_buffer, m_fullBitLength >> 3, newByteSize);
            m_fullBitLength = m_bitWriteLength = newByteSize * 8;

            return true;
        }

        public void Dispose()
        {
            DeallocateInnerBuffer();
        }
    }
}
