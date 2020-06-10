using BitSerializer;
using NUnit.Framework;

namespace BlittableTests.Bitstream
{
    [TestFixture]
    public class PeekTest
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
        public void PeekIntTest(int value)
        {
            int min = -2000, max = 0;

            m_stream.WriteInt32(value, min, max);
            m_stream.ResetRead();
            Assert.AreEqual(value, m_stream.PeekInt32(min, max));
            Assert.AreEqual(0, m_stream.BitOffset);
        }

        [TestCase(351)]
        public void PeekLong(int value)
        {
            uint min = 0, max = 500;
            m_stream.WriteUInt32((uint)value, min, max);
            m_stream.ResetRead();
            Assert.AreEqual(value, m_stream.PeekUInt32(min, max));
            Assert.AreEqual(0, m_stream.BitOffset);
        }

        [TestCase(1.4f)]
        public void PeekFloat(float value)
        {
            float min = -5, max = 5, prec = 0.2f;

            m_stream.WriteSingle(value, min, max, prec);
            m_stream.ResetRead();
            Assert.AreEqual(value, m_stream.PeekSingle(min, max, prec), 0.000005f);
            Assert.AreEqual(0, m_stream.BitOffset);
        }
    }
}
