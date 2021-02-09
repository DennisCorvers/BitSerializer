using BitSerializer.Utils;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Text;

namespace BitSerializerTests.Utils
{
    public unsafe class MemoryTests
    {
        [Test]
        public void ReallocTest()
        {
            var arr = CreateArray();

            arr = (byte*)Memory.Realloc((IntPtr)arr, 8, 16);

            // Check the original bytes.
            for (int i = 0; i < 8; i++)
                Assert.AreEqual(i, arr[i]);

            Memory.Free(arr);
        }

        [Test]
        public void ZeroMemTest()
        {
            var arr = CreateArray();

            Memory.ZeroMem(arr, 8);
            for (int i = 0; i < 8; i++)
                Assert.AreEqual(0, arr[i]);

            Memory.Free(arr);
        }

        [Test]
        public void ReallocZeroedTest()
        {
            var arr = CreateArray();

            arr = (byte*)Memory.ReallocZeroed((IntPtr)arr, 8, 16);

            // Check the original bytes.
            for (int i = 0; i < 8; i++)
                Assert.AreEqual(i, arr[i]);

            // Verify that the remainder is zeroed.
            for (int i = 8; i < 16; i++)
                Assert.AreEqual(0, arr[i]);

            Memory.Free(arr);
        }

        private byte* CreateArray()
        {
            var arr = (byte*)Memory.Alloc(8);
            for (int i = 0; i < 8; i++)
            {
                arr[i] = (byte)i;
            }

            return arr;
        }
    }
}
