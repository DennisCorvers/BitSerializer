using BitSerializer;
using BitSerializer.Extension;
using NUnit.Framework;
using System;
using System.Numerics;

namespace BitSerializer.Bitstream
{
    [TestFixture]
    internal unsafe class BlittableTypeTest
    {
        private BitStream m_stream = new BitStream();
        private Vector3 m_testStruct;
        private int m_size = sizeof(Vector3);
        private Random m_random;


        [OneTimeSetUp]
        public void Setup()
        {
            m_random = new Random();
        }

        [SetUp]
        public void Init()
        {
            m_stream.ResetWrite(64);
            m_testStruct = new Vector3(m_random.NextSingle(), m_random.NextSingle(), m_random.NextSingle());
        }


        [OneTimeTearDown]
        public void TearDown()
        { m_stream.Dispose(); }

        [Test]
        public void VectorWriteTest()
        {
            m_stream.WriteBlit(m_testStruct);
            Assert.AreEqual(m_size, m_stream.ByteOffset);
            Assert.AreEqual(m_size * 8, m_stream.BitOffset);
        }

        [Test]
        public void VectorReadTest()
        {
            m_stream.WriteBlit(m_testStruct);
            m_stream.ResetRead();

            Vector3 result = m_stream.ReadBlit<Vector3>();
            Assert.AreEqual(m_testStruct, result);
        }

        [Test]
        public void VectorSerializeTest()
        {
            m_stream.SerializeBlit(ref m_testStruct);
            Assert.AreEqual(m_size * 8, m_stream.BitOffset);

            m_stream.ResetRead();
            Vector3 result = new Vector3();
            m_stream.SerializeBlit(ref result);
            Assert.AreEqual(m_testStruct, result);
        }

        [Test]
        public void VectorOffsetTest()
        {
            m_stream.WriteULong(0, 1);
            m_stream.WriteBlit(m_testStruct);
            Assert.AreEqual(m_size * 8 + 1, m_stream.BitOffset);

            m_stream.ResetRead();
            m_stream.Zeroes(1);
            Vector3 result = m_stream.ReadBlit<Vector3>();
            Assert.AreEqual(m_testStruct, result);
        }
    }
}
