using BitSerializer.Utils;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace BitSerializer
{
    public unsafe partial class BitStreamer
    {
        #region Float-Based Read      
        public Vector2 ReadVector2()
        {
            EnsureReadSize(sizeof(float) * 8 * 2);

            return new Vector2(
                ReadFloatUnchecked(),
                ReadFloatUnchecked());
        }

        public Vector3 ReadVector3()
        {
            EnsureReadSize(sizeof(float) * 8 * 3);

            return new Vector3(
                ReadFloatUnchecked(),
                ReadFloatUnchecked(),
                ReadFloatUnchecked());
        }

        public Vector4 ReadVector4()
        {
            EnsureReadSize(sizeof(float) * 8 * 4);

            return new Vector4(
                ReadFloatUnchecked(),
                ReadFloatUnchecked(),
                ReadFloatUnchecked(),
                ReadFloatUnchecked());
        }

        public Quaternion ReadQuaternion()
        {
            EnsureReadSize(sizeof(float) * 8 * 4);

            return new Quaternion(
                ReadFloatUnchecked(),
                ReadFloatUnchecked(),
                ReadFloatUnchecked(),
                ReadFloatUnchecked());
        }


        public Vector2 ReadVector2(float min, float max, float precision)
        {
            int numBits = MathUtils.BitsRequired(min, max, precision);
            EnsureReadSize(numBits * 2);

            return new Vector2(
                ReadFloatUnchecked(min, precision, numBits),
                ReadFloatUnchecked(min, precision, numBits));
        }

        public Vector3 ReadVector3(float min, float max, float precision)
        {
            int numBits = MathUtils.BitsRequired(min, max, precision);
            EnsureReadSize(numBits * 3);

            return new Vector3(
                ReadFloatUnchecked(min, precision, numBits),
                ReadFloatUnchecked(min, precision, numBits),
                ReadFloatUnchecked(min, precision, numBits));
        }

        public Vector4 ReadVector4(float min, float max, float precision)
        {
            int numBits = MathUtils.BitsRequired(min, max, precision);
            EnsureReadSize(numBits * 4);

            return new Vector4(
                ReadFloatUnchecked(min, precision, numBits),
                ReadFloatUnchecked(min, precision, numBits),
                ReadFloatUnchecked(min, precision, numBits),
                ReadFloatUnchecked(min, precision, numBits));
        }

        public Quaternion ReadQuaternion(float min, float max, float precision)
        {
            int numBits = MathUtils.BitsRequired(min, max, precision);
            EnsureReadSize(numBits * 4);

            return new Quaternion(
                ReadFloatUnchecked(min, precision, numBits),
                ReadFloatUnchecked(min, precision, numBits),
                ReadFloatUnchecked(min, precision, numBits),
                ReadFloatUnchecked(min, precision, numBits));
        }


        public Vector2 ReadVector2Half()
        {
            EnsureReadSize(sizeof(float) * 4 * 2);

            return new Vector2(
                ReadHalfUnchecked(),
                ReadHalfUnchecked());
        }

        public Vector3 ReadVector3Half()
        {
            EnsureReadSize(sizeof(float) * 4 * 3);

            return new Vector3(
                ReadHalfUnchecked(),
                ReadHalfUnchecked(),
                ReadHalfUnchecked());
        }

        public Vector4 ReadVector4Half()
        {
            EnsureReadSize(sizeof(float) * 4 * 4);

            return new Vector4(
                ReadHalfUnchecked(),
                ReadHalfUnchecked(),
                ReadHalfUnchecked(),
                ReadHalfUnchecked());
        }

        public Quaternion ReadQuaternionHalf()
        {
            EnsureReadSize(sizeof(float) * 4 * 4);

            return new Quaternion(
                ReadHalfUnchecked(),
                ReadHalfUnchecked(),
                ReadHalfUnchecked(),
                ReadHalfUnchecked());
        }
        #endregion

        #region Int-Based Read
        public Vector2Int ReadVector2Int(int bitCount = 32)
        {
            EnsureReadSize(bitCount * 2);

            return new Vector2Int(
                ReadIntUnchecked(bitCount),
                ReadIntUnchecked(bitCount));
        }

        public Vector3Int ReadVector3Int(int bitCount = 32)
        {
            EnsureReadSize(bitCount * 3);

            return new Vector3Int(
                ReadIntUnchecked(bitCount),
                ReadIntUnchecked(bitCount),
                ReadIntUnchecked(bitCount));
        }

        public Vector2Int ReadVector2Int(int min, int max)
        {
            int numBits = MathUtils.BitsRequired(min, max);
            EnsureReadSize(numBits * 2);

            return new Vector2Int(
                ReadIntUnchecked(min, numBits),
                ReadIntUnchecked(min, numBits));
        }

        public Vector3Int ReadVector3Int(int min, int max)
        {
            int numBits = MathUtils.BitsRequired(min, max);
            EnsureReadSize(numBits * 3);

            return new Vector3Int(
                ReadIntUnchecked(min, numBits),
                ReadIntUnchecked(min, numBits),
                ReadIntUnchecked(min, numBits));
        }
        #endregion

        #region Float-Based Write
        public BitStreamer WriteVector2(Vector2 value)
        {
            EnsureWriteSize(sizeof(float) * 8 * 2);

            WriteFloatUnchecked(value.x);
            WriteFloatUnchecked(value.y);

            return this;
        }

        public BitStreamer WriteVector3(Vector3 value)
        {
            EnsureWriteSize(sizeof(float) * 8 * 3);

            WriteFloatUnchecked(value.x);
            WriteFloatUnchecked(value.y);
            WriteFloatUnchecked(value.z);

            return this;
        }

        public BitStreamer WriteVector4(Vector4 value)
        {
            EnsureWriteSize(sizeof(float) * 8 * 4);

            WriteFloatUnchecked(value.x);
            WriteFloatUnchecked(value.y);
            WriteFloatUnchecked(value.z);
            WriteFloatUnchecked(value.w);

            return this;
        }

        public BitStreamer WriteQuaternion(Quaternion value)
        {
            EnsureWriteSize(sizeof(float) * 8 * 4);

            WriteFloatUnchecked(value.x);
            WriteFloatUnchecked(value.y);
            WriteFloatUnchecked(value.z);
            WriteFloatUnchecked(value.w);

            return this;
        }


        public BitStreamer WriteVector2(Vector2 value, float min, float max, float precision)
        {
            int numBits = MathUtils.BitsRequired(min, max, precision, out float inv);
            EnsureWriteSize(numBits * 2);

            WriteFloatUnchecked(value.x, min, inv, numBits);
            WriteFloatUnchecked(value.y, min, inv, numBits);

            return this;
        }

        public BitStreamer WriteVector3(Vector3 value, float min, float max, float precision)
        {
            int numBits = MathUtils.BitsRequired(min, max, precision, out float inv);
            EnsureWriteSize(numBits * 3);

            WriteFloatUnchecked(value.x, min, inv, numBits);
            WriteFloatUnchecked(value.y, min, inv, numBits);
            WriteFloatUnchecked(value.z, min, inv, numBits);

            return this;
        }

        public BitStreamer WriteVector4(Vector4 value, float min, float max, float precision)
        {
            int numBits = MathUtils.BitsRequired(min, max, precision, out float inv);
            EnsureWriteSize(numBits * 4);

            WriteFloatUnchecked(value.x, min, inv, numBits);
            WriteFloatUnchecked(value.y, min, inv, numBits);
            WriteFloatUnchecked(value.z, min, inv, numBits);
            WriteFloatUnchecked(value.w, min, inv, numBits);

            return this;
        }

        public BitStreamer WriteQuaternion(Quaternion value, float min, float max, float precision)
        {
            int numBits = MathUtils.BitsRequired(min, max, precision, out float inv);
            EnsureWriteSize(numBits * 4);

            WriteFloatUnchecked(value.x, min, inv, numBits);
            WriteFloatUnchecked(value.y, min, inv, numBits);
            WriteFloatUnchecked(value.z, min, inv, numBits);
            WriteFloatUnchecked(value.w, min, inv, numBits);

            return this;
        }


        public BitStreamer WriteVector2Half(Vector2 value)
        {
            EnsureWriteSize(sizeof(float) * 4 * 2);

            WriteHalfUnchecked(value.x);
            WriteHalfUnchecked(value.y);

            return this;
        }

        public BitStreamer WriteVector3Half(Vector3 value)
        {
            EnsureWriteSize(sizeof(float) * 4 * 3);

            WriteHalfUnchecked(value.x);
            WriteHalfUnchecked(value.y);
            WriteHalfUnchecked(value.z);

            return this;
        }

        public BitStreamer WriteVector4Half(Vector4 value)
        {
            EnsureWriteSize(sizeof(float) * 4 * 4);

            WriteHalfUnchecked(value.x);
            WriteHalfUnchecked(value.y);
            WriteHalfUnchecked(value.z);
            WriteHalfUnchecked(value.w);

            return this;
        }

        public BitStreamer WriteQuaternionHalf(Quaternion value)
        {
            EnsureWriteSize(sizeof(float) * 4 * 4);

            WriteHalfUnchecked(value.x);
            WriteHalfUnchecked(value.y);
            WriteHalfUnchecked(value.z);
            WriteHalfUnchecked(value.w);

            return this;
        }
        #endregion

        #region Int-Based Write
        public BitStreamer WriteVector2Int(Vector2Int value, int bitCount = 32)
        {
            EnsureWriteSize(bitCount * 2);

            WriteIntUnchecked(value.x, bitCount);
            WriteIntUnchecked(value.y, bitCount);

            return this;
        }

        public BitStreamer WriteVector3Int(Vector3Int value, int bitCount = 32)
        {
            EnsureWriteSize(bitCount * 3);

            WriteIntUnchecked(value.x, bitCount);
            WriteIntUnchecked(value.y, bitCount);
            WriteIntUnchecked(value.z, bitCount);

            return this;
        }

        public BitStreamer WriteVector2Int(Vector2Int value, int min, int max)
        {
            int numBits = MathUtils.BitsRequired(min, max);
            EnsureWriteSize(numBits * 2);

            WriteIntUnchecked(value.x, min, numBits);
            WriteIntUnchecked(value.y, min, numBits);

            return this;
        }

        public BitStreamer WriteVector3Int(Vector3Int value, int min, int max)
        {
            int numBits = MathUtils.BitsRequired(min, max);
            EnsureWriteSize(numBits * 3);

            WriteIntUnchecked(value.x, min, numBits);
            WriteIntUnchecked(value.y, min, numBits);
            WriteIntUnchecked(value.z, min, numBits);

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
    }
}
