using BenchmarkDotNet.Attributes;
using BitSerializer;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace Bench
{
    [MemoryDiagnoser]
    public class StringBench
    {
        private const int AMOUNT = 1024;
        private readonly byte[] m_byteBuf;
        private readonly IntPtr m_ptrBuf;
        private const int SIZE = AMOUNT * sizeof(int) + AMOUNT;
        private BitStreamer stream = new BitStreamer();

        public StringBench()
        {
            m_byteBuf = new byte[SIZE];
            m_ptrBuf = Marshal.AllocHGlobal(SIZE);
        }
        ~StringBench()
        {
            Marshal.FreeHGlobal(m_ptrBuf);
        }

        [Benchmark]
        public void StringWrite()
        {
            stream.ResetWrite(m_ptrBuf, SIZE);

            const string str = "4Jzqh8Jb4DRlTHFjHZfAO6vRVBVLnHxEfY4Ir9lNkbRN00tn6dtRneKOKsss15PEIex";
            stream.WriteString(str, Encoding.ASCII);

            stream.ResetRead();

            var replica = stream.ReadString(Encoding.ASCII);
        }

        static char[] myString = new char[32];

        [Benchmark]
        public void UTF16Write()
        {
            stream.ResetWrite(m_ptrBuf, SIZE);

            const string str = "4Jzqh8Jb4DRlTHFjHZfAO6vRVBVLnHxEfY4Ir9lNkbRN00tn6dtRneKOKsss15PEIex";
            stream.WriteString(str, Encoding.ASCII);

            stream.ResetRead();

            var replica = stream.ReadString(myString, 0, Encoding.ASCII);
        }
    }
}
