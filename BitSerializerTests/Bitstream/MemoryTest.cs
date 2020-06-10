using BitSerializer;
using NUnit.Framework;
using System;
using System.Runtime.InteropServices;

namespace BlittableTests.Bitstream
{
    [TestFixture]
    internal unsafe class MemoryTest
    {
        private BitStream m_stream = new BitStream();

        [SetUp]
        public void Init()
        {
            m_stream.ResetWrite(64);
        }
        [OneTimeTearDown]
        public void TearDown()
        { m_stream.Dispose(); }

        [TestCase(100)]
        [TestCase(int.MinValue / 2)]
        public void MemoryTestOnce(int value)
        {
            int* ptr = &value;
            m_stream.WriteMemory(ptr, 4);
            Assert.AreEqual(4, m_stream.ByteOffset);
            m_stream.ResetRead();

            int* rep = stackalloc int[1];
            m_stream.ReadMemory(rep, 4);
            Assert.AreEqual(4, m_stream.ByteOffset);

            Assert.AreEqual(value, *rep);
        }

        public void MemoryTestMultiple()
        {
            int value = 1; double value2 = 23; long value3 = -977666665431;
            byte* dat = stackalloc byte[20];
            byte* rep = stackalloc byte[20];

            Write(value, dat);
            Write(value2, dat + 4);
            Write(value3, dat + 12);

            m_stream.WriteMemory(dat, 20);
            Assert.AreEqual(20, m_stream.ByteOffset);

            m_stream.ResetRead();
            m_stream.ReadMemory(rep, 20);
            Assert.AreEqual(20, m_stream.ByteOffset);

            Assert.AreEqual(value, Read<int>(rep));
            Assert.AreEqual(value2, Read<double>(rep + 4));
            Assert.AreEqual(value3, Read<long>(rep + 12));
        }

        [TestCase(123)]
        public void BytesTest(int value)
        {
            byte[] dat = new byte[8];
            fixed (byte* ptr = dat)
            {
                Write(value, ptr);
                Write(value * -1, ptr + 4);
            }

            m_stream.WriteBytes(dat, 0, 8, true);
            Assert.AreEqual(10, m_stream.ByteOffset);

            m_stream.ResetRead();
            byte[] rep = m_stream.ReadBytesLength();
            Assert.AreEqual(10, m_stream.ByteOffset);
            Assert.AreEqual(8, rep.Length);

            fixed (byte* ptr = rep)
            {
                Assert.AreEqual(value, Read<int>(ptr));
                Assert.AreEqual(value * -1, Read<int>(ptr + 4));
            }
        }

        [TestCase(101)]
        public void CopyToByteTest(byte value)
        {
            for (int i = 0; i < 4; i++)
            { m_stream.WriteByte((byte)(value * (i + 1))); }

            byte[] buff = new byte[4];
            m_stream.CopyTo(buff);

            for (int i = 0; i < 4; i++)
            { Assert.AreEqual(buff[i], (byte)(value * (i + 1))); }
        }

        [TestCase(99)]
        public void CopyToPtrTest(byte value)
        {
            for (int i = 0; i < 4; i++)
            { m_stream.WriteByte((byte)(value * (i + 1))); }

            byte* buff = (byte*)Marshal.AllocHGlobal(4);
            m_stream.CopyTo(buff);

            for (int i = 0; i < 4; i++)
            { Assert.AreEqual(buff[i], (byte)(value * (i + 1))); }

            Marshal.FreeHGlobal((IntPtr)buff);
        }


        private void Write<T>(T value, byte* buf) where T : unmanaged
        {
            *(T*)buf = value;
        }
        private T Read<T>(byte* buf) where T : unmanaged
        {
            return *(T*)buf;
        }
    }
}
