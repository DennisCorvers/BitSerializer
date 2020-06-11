using System;
using System.Collections.Generic;
using System.Text;

namespace BitSerializer.Extension
{
    public static class RandomExtension
    {
        public unsafe static float NextSingle(this Random random)
        {
            int next = random.Next();
            return *(float*)&next;
        }
    }
}
