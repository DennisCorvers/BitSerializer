using BitSerializer.Utils;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Text;

namespace BitSerializerTests.Utils
{
    public class MathUtilsTest
    {
        [Test]
        public void NextMultipleOf8Test()
        {
            Assert.AreEqual(0, MathUtils.GetNextMultipleOf8(7) % 8);
            Assert.AreEqual(8, MathUtils.GetNextMultipleOf8(3));
            Assert.AreEqual(8, MathUtils.GetNextMultipleOf8(8));
        }

        [Test]
        public void BitsToBytes()
        {
            Assert.AreEqual(0, 0 >> 3);
            Assert.AreEqual(0, 1 >> 3);
            Assert.AreEqual(1, 8 >> 3);
            Assert.AreEqual(8, 65 >> 3);
            Assert.AreEqual(1, 9 >> 3);
        }

        [Test]
        public void GetPreviousMultipleOf8Test()
        {
            Assert.AreEqual(8, MathUtils.GetPreviousMultipleOf8(8));
            Assert.AreEqual(0, MathUtils.GetPreviousMultipleOf8(7));
            Assert.AreEqual(8, MathUtils.GetPreviousMultipleOf8(9));
        }

        [Test]
        public void HalfFloatTest()
        {
            float original = (float)Math.PI;

            var compressed = HalfPrecision.Compress(original);
            var replica = HalfPrecision.Decompress(compressed);


            Assert.AreEqual(original, replica, 0.001f);
        }
    }
}
