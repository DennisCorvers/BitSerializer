using System.Runtime.CompilerServices;

namespace BitSerializer.Utils
{
    public unsafe static class HalfPrecision
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ushort Compress(float value)
        {
            return F32BitsToF16Bits(*(uint*)&value);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float Decompress(ushort value)
        {
            var retVal = F16BitsToF32Bits(value);
            return *(float*)&retVal;
        }

        private static ushort F32BitsToF16Bits(uint value)
        {
            // Translated from Go to C# by Dennis Corvers (github.com/DennisCorvers).
            // Translated from Rust to Go by Montgomery Edwards⁴⁴⁸ (github.com/x448).
            // All 4.294.967.296 conversions with this were confirmed to be correct by x448.
            // Original Rust implementation is by Kathryn Long (github.com/starkat99) with MIT license.

            uint sign = value & 0x80000000;
            uint exp = value & 0x7f800000;
            uint coef = value & 0x007fffff;

            if (exp == 0x7f800000)
            {
                // NaN or Infinity
                uint nanBit = 0;

                if (coef != 0)
                    nanBit = 0x0200;

                return (ushort)((sign >> 16) | 0x7c00 | nanBit | (coef >> 13));
            }

            uint halfSign = sign >> 16;
            int unbiasedExp = (int)(exp >> 23) - 127;
            int halfExp = unbiasedExp + 15;


            if (halfExp >= 0x1f)
            {
                return (ushort)(halfSign | 0x7c00);
            }

            if (halfExp <= 0)
            {
                if (14 - halfExp > 24)
                {
                    return (ushort)halfSign;
                }

                uint c = coef | 0x00800000;
                uint halfCoef = c >> (14 - halfExp);
                uint roundBit = (uint)1 << (13 - halfExp);

                if ((c & roundBit) != 0 && (c & (3 * roundBit - 1)) != 0)
                    halfCoef++;

                return (ushort)(halfSign | halfCoef);
            }

            uint uHalfExp = (uint)halfExp << 10;
            uint uhalfCoef = coef >> 13;
            uint uroundBit = 0x00001000;

            if ((coef & uroundBit) != 0 && (coef & (3 * uroundBit - 1)) != 0)
            {
                return (ushort)((halfSign | uHalfExp | uhalfCoef) + 1);
            }

            return (ushort)(halfSign | uHalfExp | uhalfCoef);
        }

        private static uint F16BitsToF32Bits(ushort value)
        {
            // All 65.536 conversions with this were confirmed to be correct
            // by Montgomery Edwards⁴⁴⁸ (github.com/x448).

            uint sign = (uint)(value & 0x8000) << 16; // sign for 32-bit
            uint exp = (uint)(value & 0x7c00) >> 10;  // exponenent for 16-bit
            uint coef = (uint)(value & 0x03ff) << 13; // significand for 32-bit

            if (exp == 0x1f)
            {
                if (coef == 0)
                {
                    // infinity
                    return sign | 0x7f800000 | coef;
                }
                // NaN
                return sign | 0x7fc00000 | coef;
            }

            if (exp == 0)
            {
                if (coef == 0)
                {
                    // zero
                    return sign;
                }

                // normalize subnormal numbers
                exp++;


                while ((coef & 0x7f800000) == 0)
                {
                    coef <<= 1;
                    exp--;
                }

                coef &= 0x007fffff;
            }

            return sign | ((exp + (0x7f - 0xf)) << 23) | coef;
        }
    }
}
