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

            const string str = "1LEFiWTEkFR1acY5mLb915mnIdvumHTiGADno8DaQAggFs0HxlHolmHoB0Mm3phKo85ahUjL4V6utXuWr6YdK3ANIuTX2vZmcx0gP9QNdqjguWFe8WDWDj91Hc0lqVBix8bNrAERgqrgkgFpe4IUXckAqWik4tlMuhLpWoWenF4Jzqh8Jb4DRlTHFjHZfAO6vRVBVLnHxEfY4Ir9lNkbRN00tn6dtRneKOKsss15PEIexXDx4tOOETniVDuCj1OtLp3tmfguRRDeLkND0RxCKoHO7qTWngVB9myQ8upOOuBxxk2bx8OFnTkFBPzVJf1CKKxHS4xMYBlwcRCKS9JlXHZPYryCMgQM7CaRG9nNYHR4r0wzYwl2RXP49narcxCuSgDcOn5ivrGVTJhjDDJDsJpGwTgJbGDnrW9JSUnP4jl1LSdzN3gcaVkSDosl12GTROxujH8hxaC1KggZirJ1IwK68tutwr4YnXKm4veCDCVZjj7WSiBqdo1XQUiTJvBt";

            stream.WriteString(str, Encoding.ASCII);

            stream.ResetRead();

            var replica = stream.ReadString(Encoding.ASCII);
        }
    }
}
