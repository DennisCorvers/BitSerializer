using BitSerializer.Utils;
using System.Runtime.CompilerServices;
#if UNITY
using UnityEngine;


namespace BitSerializer
{
    public unsafe partial class BitStreamer
    {
#region Float-Based Read      
        public Vector2 ReadVector2()
        {
            return new Vector2(ReadFloat(), ReadFloat());
        }

        public Vector3 ReadVector3()
        {
            return new Vector3(ReadFloat(), ReadFloat(), ReadFloat());
        }

        public Vector4 ReadVector4()
        {
            return new Vector4(ReadFloat(), ReadFloat(), ReadFloat(), ReadFloat());
        }

        public Quaternion ReadQuaternion()
        {
            return new Quaternion(ReadFloat(), ReadFloat(), ReadFloat(), ReadFloat());
        }


        public Vector2 ReadVector2(float min, float max, float precision)
        {
            int numBits = MathUtils.BitsRequired(min, max, precision);

            if (!EnsureReadSize(numBits * 2))
                return default;

            return new Vector2(
                ReadFloatUnchecked(min, precision, numBits),
                ReadFloatUnchecked(min, precision, numBits));
        }

        public Vector3 ReadVector3(float min, float max, float precision)
        {
            int numBits = MathUtils.BitsRequired(min, max, precision);

            if (!EnsureReadSize(numBits * 3))
                return default;

            return new Vector3(
                ReadFloatUnchecked(min, precision, numBits),
                ReadFloatUnchecked(min, precision, numBits),
                ReadFloatUnchecked(min, precision, numBits));
        }

        public Vector4 ReadVector4(float min, float max, float precision)
        {
            int numBits = MathUtils.BitsRequired(min, max, precision);

            if (!EnsureReadSize(numBits * 4))
                return default;

            return new Vector4(
                ReadFloatUnchecked(min, precision, numBits),
                ReadFloatUnchecked(min, precision, numBits),
                ReadFloatUnchecked(min, precision, numBits),
                ReadFloatUnchecked(min, precision, numBits));
        }

        public Quaternion ReadQuaternion(float min, float max, float precision)
        {
            int numBits = MathUtils.BitsRequired(min, max, precision);

            if (!EnsureReadSize(numBits * 4))
                return default;

            return new Quaternion(
                ReadFloatUnchecked(min, precision, numBits),
                ReadFloatUnchecked(min, precision, numBits),
                ReadFloatUnchecked(min, precision, numBits),
                ReadFloatUnchecked(min, precision, numBits));
        }


        public Vector2 ReadVector2Half()
        {
            return new Vector2(ReadHalf(), ReadHalf());
        }

        public Vector3 ReadVector3Half()
        {
            return new Vector3(ReadHalf(), ReadHalf(), ReadHalf());
        }

        public Vector4 ReadVector4Half()
        {
            return new Vector4(ReadHalf(), ReadHalf(), ReadHalf(), ReadHalf());
        }

        public Quaternion ReadQuaternionHalf()
        {
            return new Quaternion(ReadHalf(), ReadHalf(), ReadHalf(), ReadHalf());
        }
#endregion

#region Int-Based Read
        public Vector2Int ReadVector2Int(int bitCount = 32)
        {
            if (!EnsureReadSize(bitCount * 2))
                return default;

            return new Vector2Int(
                ReadIntUnchecked(bitCount),
                ReadIntUnchecked(bitCount));
        }

        public Vector3Int ReadVector3Int(int bitCount = 32)
        {
            if (!EnsureReadSize(bitCount * 3))
                return default;

            return new Vector3Int(
                ReadIntUnchecked(bitCount),
                ReadIntUnchecked(bitCount),
                ReadIntUnchecked(bitCount));
        }

        public Vector2Int ReadVector2Int(int min, int max)
        {
            int numBits = MathUtils.BitsRequired(min, max);

            if (!EnsureReadSize(numBits * 2))
                return default;

            return new Vector2Int(
                ReadIntUnchecked(min, numBits),
                ReadIntUnchecked(min, numBits));
        }

        public Vector3Int ReadVector3Int(int min, int max)
        {
            int numBits = MathUtils.BitsRequired(min, max);

            if (!EnsureReadSize(numBits * 3))
                return default;

            return new Vector3Int(
                ReadIntUnchecked(min, numBits),
                ReadIntUnchecked(min, numBits),
                ReadIntUnchecked(min, numBits));
        }
#endregion

#region Float-Based Write
        public BitStreamer WriteVector2(Vector2 value)
        {
            WriteFloat(value.x);
            WriteFloat(value.y);

            return this;
        }

        public BitStreamer WriteVector3(Vector3 value)
        {
            WriteFloat(value.x);
            WriteFloat(value.y);
            WriteFloat(value.z);

            return this;
        }

        public BitStreamer WriteVector4(Vector4 value)
        {
            WriteFloat(value.x);
            WriteFloat(value.y);
            WriteFloat(value.z);
            WriteFloat(value.w);

            return this;
        }

        public BitStreamer WriteQuaternion(Quaternion value)
        {
            WriteFloat(value.x);
            WriteFloat(value.y);
            WriteFloat(value.z);
            WriteFloat(value.w);

            return this;
        }


        public BitStreamer WriteVector2(Vector2 value, float min, float max, float precision)
        {
            int numBits = MathUtils.BitsRequired(min, max, precision, out float inv);

            if (EnsureWriteSize(numBits * 2))
            {
                WriteFloatUnchecked(value.x, min, inv, numBits);
                WriteFloatUnchecked(value.y, min, inv, numBits);
            }

            return this;
        }

        public BitStreamer WriteVector3(Vector3 value, float min, float max, float precision)
        {
            int numBits = MathUtils.BitsRequired(min, max, precision, out float inv);

            if (EnsureWriteSize(numBits * 3))
            {
                WriteFloatUnchecked(value.x, min, inv, numBits);
                WriteFloatUnchecked(value.y, min, inv, numBits);
                WriteFloatUnchecked(value.z, min, inv, numBits);
            }

            return this;
        }

        public BitStreamer WriteVector4(Vector4 value, float min, float max, float precision)
        {
            int numBits = MathUtils.BitsRequired(min, max, precision, out float inv);

            if (EnsureWriteSize(numBits * 4))
            {
                WriteFloatUnchecked(value.x, min, inv, numBits);
                WriteFloatUnchecked(value.y, min, inv, numBits);
                WriteFloatUnchecked(value.z, min, inv, numBits);
                WriteFloatUnchecked(value.w, min, inv, numBits);
            }

            return this;
        }

        public BitStreamer WriteQuaternion(Quaternion value, float min, float max, float precision)
        {
            int numBits = MathUtils.BitsRequired(min, max, precision, out float inv);

            if (EnsureWriteSize(numBits * 4))
            {
                WriteFloatUnchecked(value.x, min, inv, numBits);
                WriteFloatUnchecked(value.y, min, inv, numBits);
                WriteFloatUnchecked(value.z, min, inv, numBits);
                WriteFloatUnchecked(value.w, min, inv, numBits);
            }

            return this;
        }


        public BitStreamer WriteVector2Half(Vector2 value)
        {
            WriteHalf(value.x);
            WriteHalf(value.y);

            return this;
        }

        public BitStreamer WriteVector3Half(Vector3 value)
        {
            WriteHalf(value.x);
            WriteHalf(value.y);
            WriteHalf(value.z);

            return this;
        }

        public BitStreamer WriteVector4Half(Vector4 value)
        {
            WriteHalf(value.x);
            WriteHalf(value.y);
            WriteHalf(value.z);
            WriteHalf(value.w);

            return this;
        }

        public BitStreamer WriteQuaternionHalf(Quaternion value)
        {
            WriteHalf(value.x);
            WriteHalf(value.y);
            WriteHalf(value.z);
            WriteHalf(value.w);

            return this;
        }
#endregion

#region Int-Based Write
        public BitStreamer WriteVector2Int(Vector2Int value, int bitCount = 32)
        {
            if (EnsureWriteSize(bitCount * 2))
            {
                WriteIntUnchecked(value.x, bitCount);
                WriteIntUnchecked(value.y, bitCount);
            }

            return this;
        }

        public BitStreamer WriteVector3Int(Vector3Int value, int bitCount = 32)
        {
            if (EnsureWriteSize(bitCount * 3))
            {
                WriteIntUnchecked(value.x, bitCount);
                WriteIntUnchecked(value.y, bitCount);
                WriteIntUnchecked(value.z, bitCount);
            }

            return this;
        }

        public BitStreamer WriteVector2Int(Vector2Int value, int min, int max)
        {
            int numBits = MathUtils.BitsRequired(min, max);

            if (EnsureWriteSize(numBits * 2))
            {
                WriteIntUnchecked(value.x, min, numBits);
                WriteIntUnchecked(value.y, min, numBits);
            }

            return this;
        }

        public BitStreamer WriteVector3Int(Vector3Int value, int min, int max)
        {
            int numBits = MathUtils.BitsRequired(min, max);

            if (EnsureWriteSize(numBits * 3))
            {
                WriteIntUnchecked(value.x, min, numBits);
                WriteIntUnchecked(value.y, min, numBits);
                WriteIntUnchecked(value.z, min, numBits);
            }

            return this;
        }
#endregion

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public BitStreamer Serialize(ref Vector2 value)
        {
            if (m_mode == SerializationMode.Writing) WriteVector2(value);
            else value = ReadVector2();
            return this;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public BitStreamer Serialize(ref Vector3 value)
        {
            if (m_mode == SerializationMode.Writing) WriteVector3(value);
            else value = ReadVector3();
            return this;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public BitStreamer Serialize(ref Vector4 value)
        {
            if (m_mode == SerializationMode.Writing) WriteVector4(value);
            else value = ReadVector4();
            return this;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public BitStreamer Serialize(ref Quaternion value)
        {
            if (m_mode == SerializationMode.Writing) WriteQuaternion(value);
            else value = ReadQuaternion();
            return this;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public BitStreamer Serialize(ref Vector2 value, bool halfPrecision)
        {
            if (halfPrecision)
            {
                if (m_mode == SerializationMode.Writing) WriteVector2Half(value);
                else value = ReadVector2Half();
            }
            else
            {
                if (m_mode == SerializationMode.Writing) WriteVector2(value);
                else value = ReadVector2();
            }

            return this;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public BitStreamer Serialize(ref Vector3 value, bool halfPrecision)
        {
            if (halfPrecision)
            {
                if (m_mode == SerializationMode.Writing) WriteVector3Half(value);
                else value = ReadVector3Half();
            }
            else
            {
                if (m_mode == SerializationMode.Writing) WriteVector3(value);
                else value = ReadVector3();
            }

            return this;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public BitStreamer Serialize(ref Vector4 value, bool halfPrecision)
        {
            if (halfPrecision)
            {
                if (m_mode == SerializationMode.Writing) WriteVector4Half(value);
                else value = ReadVector4Half();
            }
            else
            {
                if (m_mode == SerializationMode.Writing) WriteVector4(value);
                else value = ReadVector4();
            }

            return this;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public BitStreamer Serialize(ref Quaternion value, bool halfPrecision)
        {
            if (halfPrecision)
            {
                if (m_mode == SerializationMode.Writing) WriteQuaternionHalf(value);
                else value = ReadQuaternionHalf();
            }
            else
            {
                if (m_mode == SerializationMode.Writing) WriteQuaternion(value);
                else value = ReadQuaternion();
            }

            return this;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public BitStreamer Serialize(ref Vector2 value, float min, float max, float precision)
        {
            if (m_mode == SerializationMode.Writing) WriteVector2(value, min, max, precision);
            else value = ReadVector2(min, max, precision);
            return this;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public BitStreamer Serialize(ref Vector3 value, float min, float max, float precision)
        {
            if (m_mode == SerializationMode.Writing) WriteVector3(value, min, max, precision);
            else value = ReadVector3(min, max, precision);
            return this;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public BitStreamer Serialize(ref Vector4 value, float min, float max, float precision)
        {
            if (m_mode == SerializationMode.Writing) WriteVector4(value, min, max, precision);
            else value = ReadVector4(min, max, precision);
            return this;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public BitStreamer Serialize(ref Quaternion value, float min, float max, float precision)
        {
            if (m_mode == SerializationMode.Writing) WriteQuaternion(value, min, max, precision);
            else value = ReadQuaternion(min, max, precision);
            return this;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public BitStreamer Serialize(ref Vector2Int value, int bitCount = 32)
        {
            if (m_mode == SerializationMode.Writing) WriteVector2Int(value);
            else value = ReadVector2Int();
            return this;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public BitStreamer Serialize(ref Vector3Int value, int bitCount = 32)
        {
            if (m_mode == SerializationMode.Writing) WriteVector3Int(value);
            else value = ReadVector3Int();
            return this;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public BitStreamer Serialize(ref Vector2Int value, int min, int max)
        {
            if (m_mode == SerializationMode.Writing) WriteVector2Int(value, min, max);
            else value = ReadVector2Int(min, max);
            return this;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public BitStreamer Serialize(ref Vector3Int value, int min, int max)
        {
            if (m_mode == SerializationMode.Writing) WriteVector3Int(value, min, max);
            else value = ReadVector3Int(min, max);
            return this;
        }

#region Unchecked Operations
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private float ReadFloatUnchecked(float min, float precision, int bits)
        {
            return ReadUnchecked(bits) * precision + min;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int ReadIntUnchecked(int bitCount = 32)
        {
            return ZigZag.Zag(unchecked((uint)ReadUnchecked(bitCount)));
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int ReadIntUnchecked(int min, int bits)
        {
            return unchecked((int)((long)ReadUnchecked(bits) + min));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void WriteFloatUnchecked(float value, float min, float inv, int bits)
        {
            float adjusted = (value - min) * inv;

            WriteUnchecked((uint)(adjusted + 0.5f), bits);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void WriteIntUnchecked(int value, int bitCount = 32)
        {
            WriteUnchecked(ZigZag.Zig(value), bitCount);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void WriteIntUnchecked(int value, int min, int bits)
        {
            WriteUnchecked((ulong)(value - min), bits);
        }
#endregion
    }
}
#endif