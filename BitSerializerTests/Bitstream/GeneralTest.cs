using BitSerializer;
using BitSerializer.Utils;
using NUnit.Framework;
using System;
using System.Runtime.InteropServices;

namespace BitSerializer.Bitstream
{
    [TestFixture]
    internal class GeneralTest
    {
        [Test]
        public void CTorTest()
        {
            BitStream bs = new BitStream();
            Assert.AreEqual(0, bs.BitLength);
            Assert.AreEqual(0, bs.BitOffset);
            Assert.AreEqual(false, bs.IsWriting);
            Assert.AreEqual(false, bs.IsReading);
            Assert.AreEqual(SerializationMode.None, bs.Mode);
            Assert.AreEqual(IntPtr.Zero, bs.Buffer);
        }

        //[Test]
        public void Test()
        {
            BitStream bs = new BitStream();
            bs.ResetWrite(64);

            bs.WriteFloat(0, 0, 0, 0);


            bs.Dispose();
        }


        public class Player
        {
            System.Numerics.Vector3 position;
            System.Numerics.Vector3 rotation;

            byte mana = 100;
            ushort hp = 100;
        }

        [Test]
        public void ResetReadTest1()
        {
            BitStream bs = new BitStream();
            bs.ResetRead(new byte[10]);

            Assert.AreEqual(16, bs.ByteLength);
            Assert.AreEqual(16 << 3, bs.BitLength);
            Assert.AreEqual(0, bs.BitOffset);
            bs.Dispose();
        }

        [Test]
        public void ResetReadTest2()
        {
            BitStream bs = new BitStream();
            bs.ResetRead(new byte[22], 2, 20);

            Assert.Throws<ArgumentOutOfRangeException>(() => bs.ResetRead(new byte[22], 2, 21));

            Assert.AreEqual(24, bs.ByteLength);
            Assert.AreEqual(24 << 3, bs.BitLength);
            Assert.AreEqual(0, bs.BitOffset);
            bs.Dispose();
        }

        [Test]
        public void ResetReadTest3()
        {
            BitStream bs = new BitStream();
            Assert.Throws<ArgumentNullException>(() => bs.ResetRead((IntPtr)null, 10));

            IntPtr ptr = Memory.Alloc(30);
            bs.ResetRead(ptr, 30);

            Assert.AreEqual(32 << 3, bs.BitLength);
            Assert.AreEqual(0, bs.BitOffset);
            bs.Dispose();
        }

        [Test]
        public void ResetWriteTest1()
        {
            BitStream bs = new BitStream();
            bs.ResetWrite();

            Assert.AreEqual(BitStream.DefaultSize, bs.ByteLength);
            IntPtr ptr = bs.Buffer;
            bs.ResetWrite();

            Assert.AreEqual(ptr, bs.Buffer);
            bs.Dispose();
        }

        [Test]
        public void ResetWriteTest2()
        {
            BitStream bs = new BitStream();
            bs.ResetWrite(10);
            IntPtr ptr = bs.Buffer;
            Assert.AreEqual(16, bs.ByteLength);

            bs.ResetWrite(12);
            Assert.AreEqual(16, bs.ByteLength);
            Assert.AreEqual(ptr, bs.Buffer);
            bs.ResetWrite(17);

            Assert.AreEqual(24, bs.ByteLength);

            bs.Dispose();
        }

        [Test]
        public void DisposeTest()
        {
            BitStream bs = new BitStream();
            bs.Dispose();

            bs.ResetWrite(16);
            bs.Dispose();
            Assert.AreEqual(IntPtr.Zero, bs.Buffer);

            bs.ResetWrite(18);
            Assert.AreEqual(24, bs.ByteLength);

            bs.Dispose();
            Assert.AreEqual(IntPtr.Zero, bs.Buffer);
            Assert.AreEqual(0, bs.BitLength);
            Assert.AreEqual(0, bs.BitOffset);
            Assert.AreEqual(SerializationMode.None, bs.Mode);
        }

        [Test]
        public unsafe void ReadBufferCopyTest()
        {
            ulong value = 85830981411525;
            IntPtr buf = Marshal.AllocHGlobal(8);
            *(ulong*)buf = value;

            BitStream reader = new BitStream();
            reader.ResetRead(buf, 8, false);

            Assert.AreEqual(64, reader.BitLength);
            Assert.AreEqual(0, reader.BitOffset);

            Assert.AreEqual(value, reader.ReadULong());

            reader.Dispose();
            Assert.AreEqual(IntPtr.Zero, reader.Buffer);
        }

        [Test]
        public unsafe void WriteBufferCopyTest()
        {
            ulong value = 120938129485132;
            IntPtr buf = Marshal.AllocHGlobal(8);

            BitStream reader = new BitStream();
            reader.ResetWrite(buf, 8, false);
            reader.WriteULong(value);

            Assert.AreEqual(64, reader.BitLength);
            Assert.AreEqual(64, reader.BitOffset);

            Assert.AreEqual(value, *(ulong*)reader.Buffer);

            reader.Dispose();
        }
    }
}
