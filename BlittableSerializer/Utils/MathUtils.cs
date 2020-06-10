using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace BlittableSerializer.Utils
{
    public static class MathUtils
    {
        private static readonly byte[] TAB64 = new byte[64] {
            63,  0, 58,  1, 59, 47, 53,  2,
            60, 39, 48, 27, 54, 33, 42,  3,
            61, 51, 37, 40, 49, 18, 28, 20,
            55, 30, 34, 11, 43, 14, 22,  4,
            62, 57, 46, 52, 38, 26, 32, 41,
            50, 36, 17, 19, 29, 10, 13, 21,
            56, 45, 25, 31, 35, 16,  9, 12,
            44, 24, 15,  8, 23,  7,  6,  5};
        private static readonly byte[] TAB32 = new byte[32] {
            0,  9,  1, 10, 13, 21,  2, 29,
            11, 14, 16, 18, 22, 25,  3, 30,
            8, 12, 20, 28, 15, 17, 24,  7,
            19, 27, 23,  6, 26,  5,  4, 31};

        /// <summary>
        /// Normalizes uniform-spaced float within min/max into uint with specified number of bits.
        /// This does not preserve 0 when min = -max
        /// </summary>
        public static uint NormalizeFloat(float value, float min, float max, int bits)
        {
            int num = (1 << bits) - 1; // 255 for 8 bits
            value = (value - min) / (max - min); // Scale to 0 ~ 1
            return (uint)(value * num + 0.5f); // Scale to 0 ~ 255 and round
        }

        /// <summary>
        /// Denormalizes uint with specified number of bits into uniform-space float within min/max.
        /// This does not preserve 0 when min = -max
        /// </summary>
        public static float DenormalizeFloat(uint value, float min, float max, int bits)
        {
            int num = (1 << bits) - 1; // 255 for 8 bits
            float result = value / (float)num; // Scale to 0 ~ 1
            return min + result * (max - min); // Scale to min ~ max
        }

        /// <summary>
        /// Normalizes uniform-spaced float within min/max into uint with specified number of bits.
        /// This preserves 0 when min = -max
        /// </summary>
        public static uint NormalizeFloatCenter(float value, float min, float max, int bits)
        {
            int num = (1 << bits) - 2; // 254 for 8 bits
            value = (value - min) / (max - min); // Scale to 0 ~ 1
            return (uint)(value * num + 0.5f); // Scale to 0 ~ 254 and round
        }

        /// <summary>
        /// Denormalizes uint with specified number of bits into uniform-space float within min/max.
        /// This preserves 0 when min = -max
        /// </summary>
        public static float DenormalizeFloatCenter(uint value, float min, float max, int bits)
        {
            int num = (1 << bits) - 2; // 254 for 8 bits
            float result = value / (float)num; // Scale to 0 ~ 1
            return min + result * (max - min); // Scale to min ~ max
        }

        public static int GetDivisionCeil(int num, int div)
        {
            return (num - 1) / div + 1;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int GetNextMultiple(int num, int multiple)
        {
            return (num + multiple - 1) / multiple * multiple;
        }

        public static byte Log2_32(uint value)
        {
            value |= value >> 1;
            value |= value >> 2;
            value |= value >> 4;
            value |= value >> 8;
            value |= value >> 16;
            return TAB32[value * 0x07C4ACDD >> 27];
        }
        public static byte Log2_64(ulong value)
        {
            value |= value >> 1;
            value |= value >> 2;
            value |= value >> 4;
            value |= value >> 8;
            value |= value >> 16;
            value |= value >> 32;
            return TAB64[(value - (value >> 1)) * 0x07EDD5E59A4E28C2 >> 58];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int BitsRequired(int min, int max)
        {
            return (min == max) ? 1 : Log2_32((uint)(max - min)) + 1;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int BitsRequired(uint min, uint max)
        {
            return (min == max) ? 1 : Log2_32(max - min) + 1;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int BitsRequired(long min, long max)
        {
            return (min == max) ? 1 : Log2_64((ulong)(max - min)) + 1;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int BitsRequired(ulong min, ulong max)
        {
            return (min == max) ? 1 : Log2_64(max - min) + 1;
        }
    }
}
