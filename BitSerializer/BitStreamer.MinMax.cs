using BitSerializer.Utils;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace BitSerializer
{
    public unsafe partial class BitStreamer
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public BitStreamer Serialize(ref float value, bool halfPrecision)
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
        public BitStreamer Serialize(ref float value, float min, float max, int bits)
        {
            if (m_mode == SerializationMode.Writing) WriteFloat(value, min, max, bits);
            else value = ReadFloat(min, max, bits);
            return this;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public BitStreamer Serialize(ref float value, float min, float max, float precision)
        {
            if (m_mode == SerializationMode.Writing) WriteFloat(value, min, max, precision);
            else value = ReadFloat(min, max, precision);
            return this;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public BitStreamer Serialize(ref sbyte value, sbyte min, sbyte max)
        {
            if (m_mode == SerializationMode.Writing) WriteSByte(value, min, max);
            else value = ReadSByte(min, max);
            return this;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public BitStreamer Serialize(ref short value, short min, short max)
        {
            if (m_mode == SerializationMode.Writing) WriteShort(value, min, max);
            else value = ReadShort(min, max);
            return this;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public BitStreamer Serialize(ref int value, int min, int max)
        {
            if (m_mode == SerializationMode.Writing) WriteInt32(value, min, max);
            else value = ReadInt32(min, max);
            return this;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public BitStreamer Serialize(ref long value, long min, long max)
        {
            if (m_mode == SerializationMode.Writing) WriteLong(value, min, max);
            else value = ReadLong(min, max);
            return this;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public BitStreamer Serialize(ref byte value, byte min, byte max)
        {
            if (m_mode == SerializationMode.Writing) WriteByte(value, min, max);
            else value = ReadByte(min, max);
            return this;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public BitStreamer Serialize(ref ushort value, ushort min, ushort max)
        {
            if (m_mode == SerializationMode.Writing) WriteUShort(value, min, max);
            else value = ReadUShort(min, max);
            return this;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public BitStreamer Serialize(ref uint value, uint min, uint max)
        {
            if (m_mode == SerializationMode.Writing) WriteUInt32(value, min, max);
            else value = ReadUInt32(min, max);
            return this;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public BitStreamer Serialize(ref ulong value, ulong min, ulong max)
        {
            if (m_mode == SerializationMode.Writing) WriteULong(value, min, max);
            else value = ReadULong(min, max);
            return this;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public float ReadHalf()
        {
            return HalfPrecision.Decompress((ushort)Read(16));
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public float ReadFloat(float min, float max, int bits)
        {
            Debug.Assert(min <= max);

            var maxvalue = (1 << bits) - 1;
            float range = max - min;
            var precision = range / maxvalue;

            return Read(bits) * precision + min;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public float ReadFloat(float min, float max, float precision)
        {
            Debug.Assert(min <= max);

            int numBits = MathUtils.BitsRequired(min, max, precision);
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
            Debug.Assert(min <= max);

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
            Debug.Assert(min <= max);
            MathUtils.BitsRequired(min, max);

            return Read(MathUtils.BitsRequired(min, max)) + min;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public float PeekHalf()
        {
            return HalfPrecision.Decompress((ushort)Peek(16));
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public float PeekFloat(float min, float max, int bits)
        {
            Debug.Assert(min <= max);

            var maxvalue = (1 << bits) - 1;
            float range = max - min;
            var precision = range / maxvalue;

            return Peek(bits) * precision + min;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public float PeekFloat(float min, float max, float precision)
        {
            Debug.Assert(min <= max);

            int numBits = MathUtils.BitsRequired(min, max, precision);
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
            Debug.Assert(min <= max);

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
            Debug.Assert(min <= max);
            MathUtils.BitsRequired(min, max);

            return Peek(MathUtils.BitsRequired(min, max)) + min;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public BitStreamer WriteHalf(float value)
        {
            Write(HalfPrecision.Compress(value), 16);
            return this;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public BitStreamer WriteFloat(float value, float min, float max, float precision)
        {
            Debug.Assert(min <= max);

            int bits = MathUtils.BitsRequired(min, max, precision, out float inv);
            float adjusted = (value - min) * inv;

            Write((uint)(adjusted + 0.5f), bits);
            return this;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public BitStreamer WriteFloat(float value, float min, float max, int bits)
        {
            Debug.Assert(min <= max);

            var maxvalue = (1 << bits) - 1;

            float range = max - min;
            var precision = range / maxvalue;
            var invPrecision = 1.0f / precision;

            float adjusted = (value - min) * invPrecision;

            Write((uint)(adjusted + 0.5f), bits);
            return this;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public BitStreamer WriteSByte(sbyte value, sbyte min, sbyte max)
        {
            WriteLong(value, min, max);
            return this;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public BitStreamer WriteShort(short value, int min, int max)
        {
            WriteLong(value, min, max);
            return this;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public BitStreamer WriteInt32(int value, int min, int max)
        {
            WriteLong(value, min, max);
            return this;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public BitStreamer WriteLong(long value, long min, long max)
        {
            Debug.Assert(min <= max);
            Debug.Assert(value >= min);
            Debug.Assert(value <= max);

            Write((ulong)(value - min), MathUtils.BitsRequired(min, max));
            return this;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public BitStreamer WriteByte(byte value, byte min, byte max)
        {
            WriteULong(value, min, max);
            return this;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public BitStreamer WriteUShort(ushort value, ushort min, ushort max)
        {
            WriteULong(value, min, max);
            return this;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public BitStreamer WriteUInt32(uint value, uint min, uint max)
        {
            WriteULong(value, min, max);
            return this;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public BitStreamer WriteULong(ulong value, ulong min, ulong max)
        {
            Debug.Assert(min <= max);
            Debug.Assert(value >= min);
            Debug.Assert(value <= max);

            Write(value - min, MathUtils.BitsRequired(min, max));
            return this;
        }
    }
}
