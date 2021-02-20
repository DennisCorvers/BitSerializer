using BenchmarkDotNet.Attributes;
using BitSerializer;
using BitSerializer.Utils;
using ByteStream.Mananged;
using ByteStream.Unmanaged;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using UnityEngine;

namespace Bench
{
    [MemoryDiagnoser]
    public class SerializeBench
    {
        private const int AMOUNT = 1024;
        private readonly byte[] m_byteBuf;
        private readonly IntPtr m_ptrBuf;
        private const int SIZE = AMOUNT * sizeof(int) + AMOUNT;

        public SerializeBench()
        {
            m_byteBuf = new byte[SIZE];
            m_ptrBuf = Marshal.AllocHGlobal(SIZE);
        }
        ~SerializeBench()
        {
            Marshal.FreeHGlobal(m_ptrBuf);
        }

        //[Benchmark]
        public void ByteWrite()
        {
            ByteWriter writer = new ByteWriter(m_byteBuf);
            for (int i = 0; i < AMOUNT / 2; i++)
            {
                writer.Write(i % 2 == 0);
                writer.Write(i);
            }

            ByteReader reader = new ByteReader(m_byteBuf);
            for (int i = 0; i < AMOUNT / 2; i++)
            {
                bool b = reader.Read<bool>();
                int num = reader.Read<int>();
            }
        }

        //[Benchmark]
        public void PtrWrite()
        {
            PtrWriter writer = new PtrWriter(m_ptrBuf, SIZE);
            for (int i = 0; i < AMOUNT / 2; i++)
            {
                writer.Write(i % 2 == 0);
                writer.Write(i);
            }

            PtrReader reader = new PtrReader(m_ptrBuf, SIZE);
            for (int i = 0; i < AMOUNT / 2; i++)
            {
                bool b = reader.Read<bool>();
                int num = reader.Read<int>();
            }
        }

        //[Benchmark]
        public void BitRead()
        {
            BitStreamer stream = new BitStreamer();
            stream.ResetWrite(m_ptrBuf, SIZE, false);

            for (int i = 0; i < AMOUNT / 2; i++)
            {
                stream.WriteBool(i % 2 == 0);
                stream.WriteInt32(i);
            }

            stream.ResetRead();
            for (int i = 0; i < AMOUNT / 2; i++)
            {
                bool b = stream.ReadBool();
                int num = stream.ReadInt32();
            }
        }

        //[Benchmark(Baseline = true)]
        public void ArraySegment()
        {
            ArraySegment<byte> writer = new ArraySegment<byte>(m_byteBuf);
            int offset = 0;

            for (int i = 0; i < AMOUNT / 2; i++)
            {
                writer[offset] = (byte)(i % 2); offset++;
                for (int j = 0; j < sizeof(int); j++)
                {
                    writer[offset] = (byte)(i << j * 8);
                    offset++;
                }
            }

            offset = 0;
            ArraySegment<byte> reader = new ArraySegment<byte>(m_byteBuf);
            for (int i = 0; i < AMOUNT / 2; i++)
            {
                bool b = reader[offset] == 0; offset++;
                int result = 0;
                for (int j = 0; j < sizeof(int); j++)
                {
                    result |= writer[offset] << j * 8;
                    offset++;
                }
            }
        }

        //[Benchmark]
        public void BitConvert()
        {
            int offset = 0;
            for (int i = 0; i < AMOUNT / 2; i++)
            {
                var dat = BitConverter.GetBytes(i);
                Array.Copy(dat, 0, m_byteBuf, offset, dat.Length);
                offset += sizeof(int);
            }

            offset = 0;
            for (int i = 0; i < AMOUNT; i++)
            {
                int result = BitConverter.ToInt32(m_byteBuf, offset);
                offset += sizeof(int);
            }
        }

        //[Benchmark]
        public void VectorOld()
        {
            BitStreamer stream = new BitStreamer();
            stream.ResetWrite(m_ptrBuf, SIZE, false);
            Vector2 v = new Vector2(123.321f, 8810283094.3241827391f);

            for (int i = 0; i < 512; i++)
            {
                stream.WriteVector2(v);
            }

            stream.ResetRead();
            for (int i = 0; i < 512; i++)
            {
                var vect = stream.ReadVector2();
            }
        }

        [Benchmark]
        public void StringWrite()
        {
            BitStreamer stream = new BitStreamer();
            stream.ResetWrite(m_ptrBuf, SIZE);

            const string str = "TestString123";
            Encoding enc = Encoding.Default;

            for (int i = 0; i < 100; i++)
            {
                stream.WriteString(str, enc);
            }

            stream.ResetRead();
        }
    }
}
