/*
 *  Copyright (c) 2018 Stanislav Denisov
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
using System.Runtime.CompilerServices;

namespace BitSerializer.Utils
{
    public unsafe static class HalfPrecision
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ushort Compress(float value)
        {
            return Compress(*(int*)&value);
        }

        private static ushort Compress(int value)
        {
            int signBit = (value >> 16) & 0x00008000;
            int exponent = ((value >> 23) & 0X000000FF) - (127 - 15);
            int mantissa = value & 0X007FFFFF;

            if (exponent <= 0)
            {
                if (exponent < -10)
                    return (ushort)signBit;

                mantissa = mantissa | 0x00800000;

                int t = 14 - exponent;
                int a = (1 << (t - 1)) - 1;
                int b = (mantissa >> t) & 1;

                mantissa = (mantissa + a + b) >> t;

                return (ushort)(signBit | mantissa);
            }

            if (exponent == 0XFF - (127 - 15))
            {
                if (mantissa == 0)
                    return (ushort)(signBit | 0X7C00);

                mantissa >>= 13;

                return (ushort)(signBit | 0X7C00 | mantissa | ((mantissa == 0) ? 1 : 0));
            }

            mantissa = mantissa + 0X00000FFF + ((mantissa >> 13) & 1);

            if ((mantissa & 0x00800000) != 0)
            {
                mantissa = 0;
                exponent++;
            }

            if (exponent > 30)
                return (ushort)(signBit | 0X7C00);

            return (ushort)(signBit | (exponent << 10) | (mantissa >> 13));
        }

        public static float Decompress(ushort value)
        {
            uint result;
            uint mantissa = (uint)(value & 1023);
            uint exponent = 0XFFFFFFF2;

            if ((value & -33792) == 0)
            {
                if (mantissa != 0)
                {
                    while ((mantissa & 1024) == 0)
                    {
                        exponent--;
                        mantissa = mantissa << 1;
                    }

                    mantissa &= 0XFFFFFBFF;
                    result = (((uint)value & 0x8000) << 16) | ((exponent + 127) << 23) | (mantissa << 13);
                }
                else
                {
                    result = (uint)((value & 0x8000) << 16);
                }
            }
            else
            {
                result = (((uint)value & 0x8000) << 16) | (((((uint)value >> 10) & 0X1F) - 15 + 127) << 23) | (mantissa << 13);
            }

            // Create a local copy so we can return the value.
            float res = *(float*)&result;
            return res;
        }
    }
}
