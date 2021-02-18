using BitSerializer;
using BitSerializer.Extension;
using BitSerializerTests.Extension;
using NUnit.Framework;
using System;
using System.Numerics;

namespace BitSerializer.Bitstream
{
    [TestFixture]
    internal unsafe class BlittableTypeTest
    {
        private BitStreamer m_stream = new BitStreamer();
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
        public void VectorExtensionTest()
        {
            const int bitsize = sizeof(float) * 3 / 2 * 8; //Using 3 Half Precision floats (to bit size)
            Vector3 value = new Vector3(1, 2, 3);

            m_stream.Serialize(ref value);
            Assert.AreEqual(bitsize, m_stream.BitOffset);

            Vector3 replica = new Vector3();

            m_stream.ResetRead();
            m_stream.Serialize(ref replica);
            Assert.AreEqual(bitsize, m_stream.BitOffset);
            Assert.AreEqual(value, replica);
        }
    }
}
