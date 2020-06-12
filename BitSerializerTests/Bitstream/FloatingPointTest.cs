using BitSerializer;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Text;

namespace BitSerializer.Bitstream
{
    [TestFixture]
    public class FloatingPointTest
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


        [TestCase(3.14f)]
        public void HalfTest(float value)
        {
            m_stream.WriteHalf(value);
            Assert.AreEqual(16, m_stream.BitOffset);

            m_stream.ResetRead();
            float peek = m_stream.PeekHalf();
            Assert.AreEqual(0, m_stream.BitOffset);

            float replica = m_stream.ReadHalf();
            Assert.AreEqual(16, m_stream.BitOffset);
            Assert.AreEqual(value, replica, 0.001f);
        }

        [TestCase(float.MinValue)]
        [TestCase(float.MaxValue)]
        [TestCase((float)Math.PI)]
        public void FloatTest(float value)
        {
            float replica = 0;
            m_stream.WriteFloat(value);
            Assert.AreEqual(32, m_stream.BitOffset);

            m_stream.ResetRead();
            replica = m_stream.ReadFloat();

            Assert.AreEqual(value, replica);
        }

        [TestCase((float)Math.PI)]
        public void FloatBitsTest(float value)
        {
            const int bits = 16;
            m_stream.WriteFloat(value, 0, 5, bits);
            Assert.AreEqual(bits, m_stream.BitOffset);

            m_stream.ResetRead();
            float replica = m_stream.ReadFloat(0, 5, bits);
            Assert.AreEqual(bits, m_stream.BitOffset);
            Assert.AreEqual(value, replica, 0.0001f);
        }

        [TestCase(3.14f)]
        public void SerializeHalfTest(float value)
        {
            m_stream.Serialize(ref value, true);
            Assert.AreEqual(16, m_stream.BitOffset);

            m_stream.ResetRead();
            float replica = 0;
            m_stream.Serialize(ref replica, true);
            Assert.AreEqual(16, m_stream.BitOffset);
            Assert.AreEqual(value, replica, 0.001f);
        }

        [TestCase(float.MinValue)]
        [TestCase(float.MaxValue)]
        [TestCase((float)Math.PI)]
        public void SerializeFloatTest(float value)
        {
            float replica = 0;
            m_stream.Serialize(ref value);
            Assert.AreEqual(32, m_stream.BitOffset);

            m_stream.ResetRead();
            m_stream.Serialize(ref replica);

            Assert.AreEqual(value, replica);
        }

        [TestCase((float)Math.PI)]
        public void SerializeFloatBitsTest(float value)
        {
            const int bits = 16;
            m_stream.Serialize(ref value, 3, 15, bits);
            Assert.AreEqual(bits, m_stream.BitOffset);

            m_stream.ResetRead();

            float replica = 0;
            m_stream.Serialize(ref replica, 3, 15, bits);
            Assert.AreEqual(bits, m_stream.BitOffset);
            Assert.AreEqual(value, replica, 0.0001f);
        }
    }
}
