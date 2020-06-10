using BitSerializer.Utils;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace BitSerializer
{
    public unsafe partial class BitStream
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Serialize(ref float value, float min, float max, float precision)
        {
            if (m_mode == SerializationMode.Writing) WriteSingle(value, min, max, precision);
            else value = ReadSingle(min, max, precision);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Serialize(ref sbyte value, sbyte min, sbyte max)
        {
            if (m_mode == SerializationMode.Writing) WriteSByte(value, min, max);
            else value = ReadSByte(min, max);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Serialize(ref short value, short min, short max)
        {
            if (m_mode == SerializationMode.Writing) WriteShort(value, min, max);
            else value = ReadShort(min, max);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Serialize(ref int value, int min, int max)
        {
            if (m_mode == SerializationMode.Writing) WriteInt32(value, min, max);
            else value = ReadInt32(min, max);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Serialize(ref long value, long min, long max)
        {
            if (m_mode == SerializationMode.Writing) WriteLong(value, min, max);
            else value = ReadLong(min, max);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Serialize(ref byte value, byte min, byte max)
        {
            if (m_mode == SerializationMode.Writing) WriteByte(value, min, max);
            else value = ReadByte(min, max);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Serialize(ref ushort value, ushort min, ushort max)
        {
            if (m_mode == SerializationMode.Writing) WriteUShort(value, min, max);
            else value = ReadUShort(min, max);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Serialize(ref uint value, uint min, uint max)
        {
            if (m_mode == SerializationMode.Writing) WriteUInt32(value, min, max);
            else value = ReadUInt32(min, max);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Serialize(ref ulong value, ulong min, ulong max)
        {
            if (m_mode == SerializationMode.Writing) WriteULong(value, min, max);
            else value = ReadULong(min, max);
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public float ReadSingle(float min, float max, float precision)
        {
            float inv = 1.0f / precision;
            float maxVal = (max - min) * inv;
            int numBits = MathUtils.Log2_32((uint)(maxVal + 0.5f)) + 1;

            return Read(numBits) * precision + min;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public sbyte ReadSByte(sbyte min, sbyte max)
        {
            return unchecked((sbyte)ReadLong(min, max));
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public short ReadShort(short min, short max)
        {
            return unchecked((short)ReadLong(min, max));
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int ReadInt32(int min, int max)
        {
            return unchecked((int)ReadLong(min, max));
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public long ReadLong(long min, long max)
        {
            Debug.Assert(min < max, "Max must be greater than Min");

            return (long)Read(MathUtils.BitsRequired(min, max)) + min;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public byte ReadByte(byte min, byte max)
        {
            return unchecked((byte)ReadULong(min, max));
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ushort ReadUShort(ushort min, ushort max)
        {
            return unchecked((ushort)ReadULong(min, max));
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public uint ReadUInt32(uint min, uint max)
        {
            return unchecked((uint)ReadULong(min, max));
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ulong ReadULong(ulong min, ulong max)
        {
            Debug.Assert(min < max, "Max must be greater than Min");
            MathUtils.BitsRequired(min, max);

            return Read(MathUtils.BitsRequired(min, max)) + min;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public float PeekSingle(float min, float max, float precision)
        {
            float inv = 1.0f / precision;
            float maxVal = (max - min) * inv;
            int numBits = MathUtils.Log2_32((uint)(maxVal + 0.5f)) + 1;

            return Peek(numBits) * precision + min;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public sbyte PeekSByte(sbyte min, sbyte max)
        {
            return unchecked((sbyte)PeekLong(min, max));
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public short PeekShort(short min, short max)
        {
            return unchecked((short)PeekLong(min, max));
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int PeekInt32(int min, int max)
        {
            return unchecked((int)PeekLong(min, max));
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public long PeekLong(long min, long max)
        {
            Debug.Assert(min < max, "Max must be greater than Min");

            return (long)Peek(MathUtils.BitsRequired(min, max)) + min;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public byte PeekByte(byte min, byte max)
        {
            return unchecked((byte)PeekULong(min, max));
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ushort PeekUShort(ushort min, ushort max)
        {
            return unchecked((ushort)PeekULong(min, max));
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public uint PeekUInt32(uint min, uint max)
        {
            return unchecked((uint)PeekULong(min, max));
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ulong PeekULong(ulong min, ulong max)
        {
            Debug.Assert(min < max, "Max must be greater than Min");
            MathUtils.BitsRequired(min, max);

            return Peek(MathUtils.BitsRequired(min, max)) + min;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteSingle(float value, float min, float max, float precision)
        {
            float inv = 1.0f / precision;
            float maxVal = (max - min) * inv;
            int bits = MathUtils.Log2_32((uint)(maxVal + 0.5f)) + 1;
            float adjusted = (value - min) * inv;

            Write((uint)(adjusted + 0.5f), bits);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteSByte(sbyte value, sbyte min, sbyte max)
        {
            WriteLong(value, min, max);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteShort(short value, int min, int max)
        {
            WriteLong(value, min, max);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteInt32(int value, int min, int max)
        {
            WriteLong(value, min, max);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteLong(long value, long min, long max)
        {
            Debug.Assert(min < max, "Max must be greater than Min");
            Debug.Assert(value >= min, "Value must be at least Min");
            Debug.Assert(value <= max, "Value must be smaller than or equal to Max");

            Write((ulong)(value - min), MathUtils.BitsRequired(min, max));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteByte(byte value, byte min, byte max)
        {
            WriteULong(value, min, max);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteUShort(ushort value, ushort min, ushort max)
        {
            WriteULong(value, min, max);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteUInt32(uint value, uint min, uint max)
        {
            WriteULong(value, min, max);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteULong(ulong value, ulong min, ulong max)
        {
            Debug.Assert(min < max, "Max must be greater than Min");
            Debug.Assert(value >= min, "Value must be at least Min");
            Debug.Assert(value <= max, "Value must be smaller than or equal to Max");

            Write(value - min, MathUtils.BitsRequired(min, max));
        }
    }
}
