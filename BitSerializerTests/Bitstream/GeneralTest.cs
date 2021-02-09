using BitSerializer.Utils;
using NUnit.Framework;
using System;
using System.Runtime.InteropServices;

namespace BitSerializer.Bitstream
{
    [TestFixture]
    internal unsafe class GeneralTests
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

        [Test]
        public void SizeTest()
        {
            BitStream bs = new BitStream();
            bs.ResetWrite(60);

            // Rounded to next multiple of 8 = 64;
            Assert.AreEqual(64, bs.ByteLength);
            Assert.AreEqual(64 * 8, bs.BitLength);

            bs.WriteInt32(1, 28);
            Assert.AreEqual(28, bs.BitOffset);
            Assert.AreEqual(28 / (double)8, bs.ByteOffset);

            Assert.AreEqual(4, bs.BytesUsed);
        }

        [Test]
        public void ResetWithoutBufferTest()
        {
            BitStream bs = new BitStream();
            Assert.Throws<InvalidOperationException>(() =>
            {
                bs.ResetRead();
            });
        }

        [Test]
        public void ResetReadTest1()
        {
            BitStream bs = new BitStream();
            bs.ResetRead(new byte[10]);

            Assert.AreEqual(16, bs.ByteLength);
            Assert.AreEqual(16 << 3, bs.BitLength);
            Assert.AreEqual(0, bs.BitOffset);
            Assert.IsTrue(bs.OwnsBuffer);
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

            IntPtr ptr = Marshal.AllocHGlobal(30);
            bs.ResetRead(ptr, 30, true);

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
            Assert.AreEqual(IntPtr.Zero, bs.Buffer);
        }

        [Test]
        public void ResetWriteInvalid()
        {
            BitStream bs = new BitStream();

            IntPtr buff = Memory.Alloc(16);
            *(byte*)buff = 212;

            bs.ResetWrite(8);

            Assert.Throws<InvalidOperationException>(() =>
            {
                bs.ResetWrite(buff, 16);
            });

        }

        [Test]
        public void ResetWriteCopyBufferNull()
        {
            BitStream bs = new BitStream();

            IntPtr buff = Memory.Alloc(16);
            *(byte*)buff = 212;

            bs.ResetWrite(buff, 16, true);
            Assert.AreEqual(16, bs.ByteLength);
            Assert.AreEqual(16, bs.ByteOffset);

            bs.ResetRead();
            Assert.AreEqual(212, bs.ReadByte());
            Assert.IsTrue(bs.OwnsBuffer);

            Memory.Free(buff);
        }

        [Test]
        public void ResetWriteCopyBufferTooSmall()
        {
            BitStream bs = new BitStream();

            IntPtr buff = Memory.Alloc(16);
            *(byte*)buff = 212;

            bs.ResetWrite(8);

            Assert.AreEqual(8, bs.ByteLength);

            bs.ResetWrite(buff, 16, true);
            Assert.AreEqual(16, bs.ByteLength);
            Assert.AreEqual(16, bs.ByteOffset);

            bs.ResetRead();
            Assert.AreEqual(212, bs.ReadByte());
            Assert.IsTrue(bs.OwnsBuffer);

            Memory.Free(buff);
        }

        [Test]
        public void ResetWriteCopy()
        {
            BitStream bs = new BitStream();

            IntPtr buff = Memory.Alloc(16);
            *(byte*)buff = 212;

            bs.ResetWrite(32);

            Assert.AreEqual(32, bs.ByteLength);

            bs.ResetWrite(buff, 16, true);
            Assert.AreEqual(32, bs.ByteLength);
            Assert.AreEqual(16, bs.ByteOffset);

            bs.ResetRead();
            Assert.AreEqual(212, bs.ReadByte());
            Assert.IsTrue(bs.OwnsBuffer);

            Memory.Free(buff);
        }

        [Test]
        public void ResetWriteOwnsBufferTest()
        {
            BitStream bs = new BitStream();
            bs.ResetWrite(16);
            bs.WriteByte(1);
            bs.WriteByte(2);

            Assert.AreEqual(16, bs.BitOffset);
            Assert.AreEqual(16, bs.ByteLength);

            // Confirm values are there.
            bs.ResetRead();
            Assert.AreEqual(1, bs.ReadByte());
            Assert.AreEqual(2, bs.ReadByte());

            bs.ResetWrite();
            Assert.AreEqual(0, bs.BitOffset);
            Assert.AreEqual(16, bs.ByteLength);

            // Confirm values have been zeroed.
            bs.ResetRead();
            Assert.AreEqual(0, bs.ReadByte());
        }

        [Test]
        public void ResetReadOffsetTest()
        {
            var arr = new byte[16];
            arr[5] = 123;

            BitStream bs = new BitStream();
            bs.ResetRead(arr, 5, 10);

            Assert.AreEqual(16, bs.ByteLength);
            Assert.AreEqual(0, bs.ByteOffset);

            Assert.AreEqual(123, bs.ReadByte());
        }

        [Test]
        public void DisposeWriterTest()
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
        }

        [Test]
        public unsafe void WriteBufferCopyTest()
        {
            ulong value = 666;
            IntPtr buf = Marshal.AllocHGlobal(8);

            BitStream reader = new BitStream();
            reader.ResetWrite(buf, 8, false);
            reader.WriteULong(value, 64);

            Assert.AreEqual(64, reader.BitLength);
            Assert.AreEqual(64, reader.BitOffset);

            Assert.AreEqual(value, *(ulong*)reader.Buffer);

            reader.Dispose();
        }

        [Test]
        public unsafe void SizePrefixTest()
        {
            BitStream bs = new BitStream();
            bs.ResetWrite(64);

            bs.ReserveSizePrefix();

            Assert.AreEqual(4, bs.ByteOffset);

            // Write some random data.
            var bits = 32;
            for (int i = 0; i < 8; i++)
            {
                bs.WriteInt32(i + 1, 7);
                bits += 7;
            }
            Assert.AreEqual(bits, bs.BitOffset);

            int bytesUsed = bs.BytesUsed;

            // Prefix the size and make sure the offset remains unchanged.
            Assert.AreEqual(bytesUsed, bs.PrefixSize());
            Assert.AreEqual(bits, bs.BitOffset);

            var newbs = new BitStream();
            newbs.ResetRead(bs.Buffer, bs.ByteLength, false);

            // Read the length of the buffer.
            // Must be read as uint due to Zig/Zagging of int value.
            Assert.AreEqual(bytesUsed, newbs.ReadUInt32());

            for (int i = 0; i < 8; i++)
                Assert.AreEqual(i + 1, newbs.ReadInt32(7));

            Assert.AreEqual(bs.BitOffset, newbs.BitOffset);
        }

        [Test]
        public void ExpandFailTest()
        {
            BitStream bs = new BitStream();
            IntPtr ptr = Marshal.AllocHGlobal(9);

            bs.ResetWrite(ptr, 9, false);
            Assert.AreEqual(8, bs.ByteLength);

            bs.WriteLong(1);

            Assert.Throws<InvalidOperationException>(() =>
            {
                bs.WriteLong(2);
            });

            Assert.AreEqual(8, bs.ByteLength);
        }

        [Test]
        public void ExpandTest()
        {
            BitStream bs = new BitStream();

            bs.ResetWrite(7);
            Assert.AreEqual(8, bs.ByteLength);

            bs.WriteLong(1);
            bs.WriteLong(2);

            Assert.AreEqual(16, bs.ByteLength);
        }

        [Test]
        public void ZeroLargeTest()
        {
            BitStream bs = new BitStream();
            bs.ResetWrite(8);

            Assert.AreEqual(8, bs.ByteLength);

            bs.Zeroes(20 << 3);

            Assert.AreEqual(24, bs.ByteLength);
        }

        [Test]
        public void CopyWithoutResize()
        {
            BitStream bs = new BitStream();
            bs.ResetWrite(64);

            Assert.AreEqual(64, bs.ByteLength);

            bs.ResetRead(new byte[16]);
            Assert.AreEqual(64, bs.ByteLength);
            Assert.IsTrue(bs.OwnsBuffer);

            bs.Dispose();
        }

        [Test]
        public void CopyWithResize()
        {
            BitStream bs = new BitStream();
            bs.ResetWrite(8);

            Assert.AreEqual(8, bs.ByteLength);

            bs.ResetRead(new byte[16]);
            Assert.AreEqual(16, bs.ByteLength);
            Assert.IsTrue(bs.OwnsBuffer);

            bs.Dispose();
        }
    }
}
