using BitSerializer;
using NUnit.Framework;
using System.Text;

namespace BitSerializer.Bitstream
{
    [TestFixture]
    internal class StringsTest
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

        [TestCase("MyString")]
        public void ASCIITest(string value)
        {
            m_stream.WriteString(value, Encoding.ASCII);
            Assert.AreEqual(sizeof(ushort) + value.Length, m_stream.ByteOffset);

            m_stream.ResetRead();
            Assert.AreEqual(value, m_stream.ReadString(Encoding.ASCII));
        }
        [TestCase("手機瀏覽")]
        public void UTF16Test(string value)
        {
            m_stream.WriteString(value, Encoding.Unicode);
            Assert.AreEqual(sizeof(ushort) + value.Length * 2, m_stream.ByteOffset);

            m_stream.ResetRead();
            Assert.AreEqual(value, m_stream.ReadString(Encoding.Unicode));
        }

        [TestCase("手機瀏覽")]
        [TestCase("MyString")]
        public void UTF8Test(string value)
        {
            m_stream.WriteString(value, Encoding.UTF8);
            m_stream.ResetRead();

            Assert.AreEqual(value, m_stream.ReadString(Encoding.UTF8));
        }

        [TestCase("手機瀏覽")]
        [TestCase("HelloWorld!")]
        public void CharArrayTest(string value)
        {
            char[] arr = value.ToCharArray();
            char[] rep = new char[16];

            m_stream.WriteString(arr, Encoding.UTF32);
            m_stream.ResetRead();

            int charCount = m_stream.ReadString(rep, 0, Encoding.UTF32);

            Assert.AreEqual(charCount, value.Length);
            string repStr = new string(rep, 0, charCount);
            Assert.AreEqual(value, repStr);
        }

        [TestCase("手機瀏覽")]
        [TestCase("HelloWorld!")]
        public void StringSerializeTest(string value)
        {
            string replica = "";
            m_stream.Serialize(ref value, Encoding.UTF32);
            m_stream.ResetRead();

            m_stream.Serialize(ref replica, Encoding.UTF32);

            Assert.AreEqual(value, replica);
        }

        [Test]
        public void FStringASCIITest()
        {
            string value = "12345678";

            m_stream.WriteASCII(value);
            Assert.AreEqual(10, m_stream.ByteOffset);

            m_stream.ResetRead();

            Assert.AreEqual(value, m_stream.ReadASCII());
            Assert.AreEqual(10, m_stream.ByteOffset);
        }

        [Test]
        public void FStringUTF16Test()
        {
            string value = "12345678";

            m_stream.WriteUTF16(value);
            Assert.AreEqual(18, m_stream.ByteOffset);

            m_stream.ResetRead();

            Assert.AreEqual(value, m_stream.ReadUTF16());
            Assert.AreEqual(18, m_stream.ByteOffset);
        }

        [TestCase("手機瀏覽")]
        [TestCase("HelloWorld!")]
        public void FUTF16SpecialCharTest(string value)
        {
            m_stream.WriteUTF16(value);
            Assert.AreEqual(sizeof(ushort) + value.Length * 2, m_stream.ByteOffset);

            m_stream.ResetRead();
            Assert.AreEqual(value, m_stream.ReadString(Encoding.Unicode));
        }

        [TestCase("手機瀏覽", FastEncoding.UTF16)]
        [TestCase("HelloWorld!", FastEncoding.UTF16)]
        [TestCase("HelloWorld!", FastEncoding.ASCII)]
        public void FStringSerializeTest(string value, FastEncoding encoding)
        {
            string replica = "";
            m_stream.Serialize(ref value, encoding);
            m_stream.ResetRead();

            m_stream.Serialize(ref replica, encoding);

            Assert.AreEqual(value, replica);
        }
    }
}
