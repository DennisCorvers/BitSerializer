using BitSerializer.Utils;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace BitSerializer
{

    /// <summary>
    /// Implements unchecked writes and reads to speed up censecutive operations
    /// </summary>
    public unsafe partial class BitStreamer
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private float ReadFloatUnchecked()
        {
            uint val = unchecked((uint)ReadUnchecked(32));
            return *(float*)&val;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private float ReadFloatUnchecked(float min, float precision, int bits)
        {
            return ReadUnchecked(bits) * precision + min;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private float ReadHalfUnchecked()
        {
            return HalfPrecision.Decompress((ushort)ReadUnchecked(16));
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
        private void WriteFloatUnchecked(float value)
        {
            WriteUnchecked(*(uint*)&value, sizeof(float) * 8);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void WriteFloatUnchecked(float value, float min, float inv, int bits)
        {
            float adjusted = (value - min) * inv;

            WriteUnchecked((uint)(adjusted + 0.5f), bits);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void WriteHalfUnchecked(float value)
        {
            WriteUnchecked(HalfPrecision.Compress(value), 16);
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
    }
}
