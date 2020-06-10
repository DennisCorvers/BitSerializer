using BenchmarkDotNet.Attributes;
using BlittableSerializer;
using ByteStream;
using ByteStream.Unmanaged;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace Bench
{
    public class ReadBench
    {
        private const int AMOUNT = 1024;
        private readonly byte[] m_byteBuf;
        private readonly IntPtr m_ptrBuf;

        public ReadBench()
        {
            int size = sizeof(int) * AMOUNT;
            m_byteBuf = new byte[size];
            m_ptrBuf = Marshal.AllocHGlobal(size);

            for (int i = 0; i < size; i += 4)
            {
                int num = i / 4;
                BinaryHelper.Write(m_byteBuf, i, num);
                BinaryHelper.Write(m_ptrBuf, i, num);
            }
        }

        ~ReadBench()
        {
            Marshal.FreeHGlobal(m_ptrBuf);
        }

        [Benchmark]
        public void PtrRead()
        {
            PtrReader reader = new PtrReader(m_ptrBuf, AMOUNT * sizeof(int));
            for (int i = 0; i < AMOUNT; i++)
            { int result = reader.Read<int>(); }
        }

        [Benchmark]
        public void BitRead()
        {
            BitStream reader = new BitStream();
            reader.ResetRead(m_ptrBuf, AMOUNT * sizeof(int), false);

            for (int i = 0; i < AMOUNT; i++)
            { int result = reader.ReadInt32(); }
        }

        [Benchmark(Baseline = true)]
        public void ArraySegment()
        {
            ArraySegment<byte> writer = new ArraySegment<byte>(m_byteBuf);
            int offset = 0;

            for (int i = 0; i < AMOUNT; i++)
            {
                int result = 0;
                for (int j = 0; j < sizeof(int); j++)
                {
                    result |= writer[offset] << j * 8;
                    offset++;
                }
            }
        }

        [Benchmark]
        public void BitConvert()
        {
            int offset = 0;
            for (int i = 0; i < AMOUNT; i++)
            {
                int result = BitConverter.ToInt32(m_byteBuf, offset);
                offset += sizeof(int);
            }
        }


    }
}
