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

            m_stream.WriteString(arr, BitEncoding.UTF16);
            m_stream.ResetRead();

            int charCount = m_stream.ReadString(rep, BitEncoding.UTF16);

            Assert.AreEqual(charCount, value.Length);
            string repStr = new string(rep, 0, charCount);
            Assert.AreEqual(value, repStr);
        }

        [Test]
        public void CharArrayOffsetTest()
        {
            const string value = "myTestString";

            char[] arr = value.ToCharArray();
            char[] rep = new char[16];

            m_stream.WriteString(arr, 2, 4, BitEncoding.UTF16);
            m_stream.ResetRead();

            int charCount = m_stream.ReadString(rep, BitEncoding.UTF16);

            Assert.AreEqual(4, charCount);
            string repStr = new string(rep, 0, charCount);
            Assert.AreEqual("Test", repStr);
        }

        [Test]
        public void CharArraySmallTest()
        {
            CharArraySmall(BitEncoding.ASCII);
            CharArraySmall(BitEncoding.UTF16);
        }

        private void CharArraySmall(BitEncoding encoding)
        {
            m_stream.ResetWrite();

            char[] rep = new char[4];

            m_stream.WriteString("TestString", encoding);
            m_stream.ResetRead();

            int charCount = m_stream.ReadString(rep, encoding);

            Assert.AreEqual(charCount, rep.Length);
            string repStr = new string(rep, 0, charCount);
            Assert.AreEqual("Test", repStr);
        }

        [TestCase("手機瀏覽")]
        [TestCase("HelloWorld!")]
        public unsafe void MixedSerializeTest(string value)
        {
            fixed (char* ptr = value)
            {
                m_stream.WriteString(ptr, value.Length, BitEncoding.UTF16);
            }

            string replica = "";
            m_stream.ResetRead();

            m_stream.Serialize(ref replica, BitEncoding.UTF16);

            Assert.AreEqual(value, replica);
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

            m_stream.WriteString(value, BitEncoding.ASCII);
            Assert.AreEqual(10, m_stream.ByteOffset);

            m_stream.ResetRead();

            Assert.AreEqual(value, m_stream.ReadString(BitEncoding.ASCII));
            Assert.AreEqual(10, m_stream.ByteOffset);
        }

        [Test]
        public void ASCIIOOBTest()
        {
            string value = "÷23456789÷";

            m_stream.WriteString(value, BitEncoding.ASCII);
            Assert.AreEqual(12, m_stream.ByteOffset);

            m_stream.ResetRead();

            Assert.AreEqual("?23456789?", m_stream.ReadString(BitEncoding.ASCII));
            Assert.AreEqual(12, m_stream.ByteOffset);
        }

        [Test]
        public void NonASCIICompressed()
        {
            string value = "÷23456789÷";
            double size = 2 + (value.Length * 7 / 8d);

            m_stream.WriteString(value, BitEncoding.ASCIICompressed);
            Assert.AreEqual(size, m_stream.ByteOffset);

            m_stream.ResetRead();

            Assert.AreNotEqual("?23456789?", m_stream.ReadString(BitEncoding.ASCIICompressed));
            Assert.AreEqual(size, m_stream.ByteOffset);
        }

        [Test]
        public void ASCIICompressed()
        {
            string value = "1234567890";
            double size = 2 + (value.Length * 7 / 8d);

            m_stream.WriteString(value, BitEncoding.ASCIICompressed);
            Assert.AreEqual(size, m_stream.ByteOffset);

            m_stream.ResetRead();

            Assert.AreEqual(value, m_stream.ReadString(BitEncoding.ASCIICompressed));
            Assert.AreEqual(size, m_stream.ByteOffset);
        }

        [Test]
        public void FStringUTF16Test()
        {
            string value = "12345678";

            m_stream.WriteString(value);
            Assert.AreEqual(18, m_stream.ByteOffset);

            m_stream.ResetRead();

            Assert.AreEqual(value, m_stream.ReadString());
            Assert.AreEqual(18, m_stream.ByteOffset);
        }

        [TestCase("手機瀏覽")]
        [TestCase("HelloWorld!")]
        public void FUTF16SpecialCharTest(string value)
        {
            m_stream.WriteString(value);
            Assert.AreEqual(sizeof(ushort) + value.Length * 2, m_stream.ByteOffset);

            m_stream.ResetRead();
            Assert.AreEqual(value, m_stream.ReadString(Encoding.Unicode));
        }

        [TestCase("手機瀏覽", BitEncoding.UTF16)]
        [TestCase("HelloWorld!", BitEncoding.UTF16)]
        [TestCase("HelloWorld!", BitEncoding.ASCII)]
        public void FStringSerializeTest(string value, BitEncoding encoding)
        {
            string replica = "";
            m_stream.Serialize(ref value, encoding);
            m_stream.ResetRead();

            m_stream.Serialize(ref replica, encoding);

            Assert.AreEqual(value, replica);
        }
    }
}
