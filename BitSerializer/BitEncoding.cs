using System;
using System.Collections.Generic;
using System.Text;

namespace BitSerializer
{
    public enum BitEncoding : byte
    {
        /// <summary>
        /// 1-byte ASCII encoding.
        /// </summary>
        ASCII,
        /// <summary>
        /// 2-byte UTF16 (Default C#) encoding.
        /// </summary>
        UTF16,
        /// <summary>
        /// 7-bit ASCII encoding.
        /// </summary>
        ASCIICompressed
    }
}
