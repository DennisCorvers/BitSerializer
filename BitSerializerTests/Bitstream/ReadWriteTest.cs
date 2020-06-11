using BitSerializer;
using NUnit.Framework;

namespace BitSerializer.Bitstream
{
    [TestFixture]
    internal class ReadWriteTest
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


        [TestCase(-1532)]
        public void ReadwriteTest(int value)
        {
            m_stream.WriteInt32(value);
            Assert.AreEqual(32, m_stream.BitOffset);

            m_stream.ResetRead();
            Assert.AreEqual(value, m_stream.ReadInt32());
            Assert.AreEqual(32, m_stream.BitOffset);
        }
        [TestCase(-1532)]
        public void ReadwriteFloat(float value)
        {
            int val = (int)value;
            m_stream.WriteByte(0, 4);
            m_stream.WriteInt32(val);

            m_stream.ResetRead();
            m_stream.ReadByte(4);
            Assert.AreEqual(val, m_stream.ReadInt32());
        }
        [Test]
        public void ReadWriteMultipleTest()
        {
            bool bVal = true;
            double dVal = double.MaxValue / 3 * 2;
            float fVal = float.MinValue / 5;
            short sVal = -12345;
            int offset = 113;

            m_stream.WriteBool(bVal);
            m_stream.WriteDouble(dVal);
            m_stream.WriteFloat(fVal);
            m_stream.WriteShort(sVal);
            Assert.AreEqual(offset, m_stream.BitOffset);

            m_stream.ResetRead();
            Assert.AreEqual(bVal, m_stream.ReadBool());
            Assert.AreEqual(dVal, m_stream.ReadDouble());
            Assert.AreEqual(fVal, m_stream.ReadFloat());
            Assert.AreEqual(sVal, m_stream.ReadShort());
            Assert.AreEqual(offset, m_stream.BitOffset);
        }
        [Test]
        public void ReadWriteSizeTest()
        {
            byte bVal = 100; int iVal = -100; const byte bitSize = 7;

            m_stream.WriteByte(bVal, bitSize);
            m_stream.WriteInt32(iVal, bitSize + 1);
            Assert.AreEqual(bitSize * 2 + 1, m_stream.BitOffset);

            m_stream.ResetRead();
            Assert.AreEqual(bVal, m_stream.ReadByte(bitSize));
            Assert.AreEqual(iVal, m_stream.ReadInt32(bitSize + 1));
            Assert.AreEqual(bitSize * 2 + 1, m_stream.BitOffset);
        }

        [TestCase(-29183742)]
        public void SerializeReadTest(int value)
        {
            m_stream.WriteInt32(value);
            m_stream.ResetRead();

            int ret = 0;
            m_stream.Serialize(ref ret);
            Assert.AreEqual(ret, value);
            Assert.AreEqual(32, m_stream.BitOffset);
        }
        [TestCase(-976)]
        public void SerializeWriteTest(int value)
        {
            m_stream.Serialize(ref value);
            Assert.AreEqual(32, m_stream.BitOffset);

            m_stream.ResetRead();
            Assert.AreEqual(value, m_stream.ReadInt32());
        }
        [TestCase(-1.6234f)]
        public void SerializeTest(float value)
        {
            m_stream.Serialize(ref value);
            Assert.AreEqual(32, m_stream.BitOffset);

            m_stream.ResetRead();
            float ret = 0;
            m_stream.Serialize(ref ret);
            Assert.AreEqual(value, ret);
            Assert.AreEqual(32, m_stream.BitOffset);
        }

        [TestCase(12514)]
        public void SkipBitsTest(int value)
        {
            m_stream.Zeroes(5);
            m_stream.WriteInt32(value);
            Assert.AreEqual(37, m_stream.BitOffset);

            m_stream.ResetRead();
            m_stream.Zeroes(5);
            Assert.AreEqual(value, m_stream.ReadInt32());
            Assert.AreEqual(37, m_stream.BitOffset);
        }
    }
}
