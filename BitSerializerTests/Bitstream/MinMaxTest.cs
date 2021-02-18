using BitSerializer;
using NUnit.Framework;

namespace BitSerializer.Bitstream
{
    [TestFixture]
    internal class MinMaxTest
    {
        private BitStreamer m_stream = new BitStreamer();

        [SetUp]
        public void Init()
        {
            m_stream.ResetWrite(64);
        }
        [OneTimeTearDown]
        public void TearDown()
        { m_stream.Dispose(); }


        [TestCase(-1532)]
        public void ReadWriteIntTest(int value)
        {
            const int min = -2000, max = 0;

            m_stream.WriteInt32(value, min, max);
            m_stream.ResetRead();
            Assert.AreEqual(value, m_stream.ReadInt32(min, max));
        }

        [TestCase(351)]
        public void ReadWriteUIntTest(int value)
        {
            const uint min = 0, max = 500;

            m_stream.WriteUInt32((uint)value, min, max);
            m_stream.ResetRead();
            Assert.AreEqual(value, m_stream.ReadUInt32(min, max));
        }

        [TestCase(1.4f)]
        public void ReadWriteFloatTest(float value)
        {
            const float min = -5, max = 5, prec = 0.2f;

            m_stream.WriteFloat(value, min, max, prec);
            m_stream.ResetRead();
            Assert.AreEqual(value, m_stream.ReadFloat(min, max, prec), 0.000005f);
        }

        [TestCase]
        public void ReadWriteMultiple()
        {
            m_stream.WriteInt32(-50, -100, 100);
            m_stream.WriteByte(98, 0, 100);
            m_stream.WriteBool(true);
            m_stream.WriteShort(-30, -50, 0);

            m_stream.ResetRead();

            Assert.AreEqual(-50, m_stream.ReadInt32(-100, 100));
            Assert.AreEqual(98, m_stream.ReadByte(0, 100));
            Assert.AreEqual(true, m_stream.ReadBool());
            Assert.AreEqual(-30, m_stream.ReadShort(-50, 0));
        }

        [TestCase(-1532)]
        public void SerializeIntTest(int value)
        {
            const int min = -2000, max = 0;
            int rep = 0;

            m_stream.Serialize(ref value, min, max);
            Assert.AreEqual(true, m_stream.BitOffset > 0);
            m_stream.ResetRead();
            m_stream.Serialize(ref rep, min, max);

            Assert.AreEqual(value, rep);
        }

        [TestCase(351)]
        public void SerializeUIntTest(int value)
        {
            const uint min = 0, max = 500;
            uint val = (uint)value, rep = 0;

            m_stream.Serialize(ref val, min, max);
            Assert.AreEqual(true, m_stream.BitOffset > 0);
            m_stream.ResetRead();
            m_stream.Serialize(ref rep, min, max);

            Assert.AreEqual(value, rep);
        }

        [TestCase]
        public void SerializeMultiple()
        {
            HelperStruct hlp = new HelperStruct(-50, 98, true, -30);

            m_stream.Serialize(ref hlp.intVal, -100, 100);
            m_stream.Serialize(ref hlp.bytVal, 0, 100);
            m_stream.Serialize(ref hlp.bolVal);
            m_stream.Serialize(ref hlp.srtVal, -50, 0);

            m_stream.ResetRead();

            HelperStruct rep = new HelperStruct();
            m_stream.Serialize(ref rep.intVal, -100, 100);
            m_stream.Serialize(ref rep.bytVal, 0, 100);
            m_stream.Serialize(ref rep.bolVal);
            m_stream.Serialize(ref rep.srtVal, -50, 0);

            Assert.AreEqual(hlp, rep);
        }

        [TestCase]
        public void SerializeBuilderTest()
        {
            long lvalue = -68171523;
            sbyte sbvalue = -100;
            ulong ulvalue = 829058234234;
            ushort usvalue = 54105;

            m_stream
                .Serialize(ref lvalue, -100000000, 0)
                .Serialize(ref sbvalue, -125, -50)
                .Serialize(ref ulvalue, 1000000000, ulong.MaxValue)
                .Serialize(ref usvalue, 10000, 60000)
                .ResetRead();

            Assert.AreEqual(SerializationMode.Reading, m_stream.Mode);

            long repLvalue = 0;
            sbyte repSbvalue = 0;
            ulong repUlvalue = 0;
            ushort repUsvalue = 0;

            m_stream
                .Serialize(ref repLvalue, -100000000, 0)
                .Serialize(ref repSbvalue, -125, -50)
                .Serialize(ref repUlvalue, 1000000000, ulong.MaxValue)
                .Serialize(ref repUsvalue, 10000, 60000);

            Assert.AreEqual(lvalue, repLvalue);
            Assert.AreEqual(sbvalue, repSbvalue);
            Assert.AreEqual(ulvalue, repUlvalue);
            Assert.AreEqual(usvalue, repUsvalue);
        }

        private struct HelperStruct
        {
            public int intVal;
            public byte bytVal;
            public bool bolVal;
            public short srtVal;

            public HelperStruct(int intVal, byte bytVal, bool bolVal, short srtVal)
            {
                this.intVal = intVal;
                this.bytVal = bytVal;
                this.bolVal = bolVal;
                this.srtVal = srtVal;
            }
        }
    }
}
