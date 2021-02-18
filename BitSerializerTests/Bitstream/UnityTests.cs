using BitSerializer;
using BitSerializer.Utils;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

namespace BitSerializerTests.Bitstream
{
    public class UnityTests
    {
        private BitStreamer stream = new BitStreamer();
        private System.Random rnd = new System.Random();

        [SetUp]
        public void Init()
        {
            stream.ResetWrite(128);
        }


        [OneTimeTearDown]
        public void TearDown()
        {
            stream.Dispose();
        }

        [Test]
        public void Vector2Test()
        {
            Vector2 value = new Vector2(rnd.NextSmallFloat(), rnd.NextSmallFloat());
            stream.Skip(1);
            stream.WriteVector2(value);

            Assert.AreEqual(65, stream.BitOffset);
            stream.ResetRead();
            stream.Skip(1);

            Assert.AreEqual(value, stream.ReadVector2());
            Assert.AreEqual(65, stream.BitOffset);
        }

        [Test]
        public void Vector3Test()
        {
            int size = sizeof(float) * 8 * 3;
            Vector3 value = new Vector3(rnd.NextSmallFloat(), rnd.NextSmallFloat(), rnd.NextSmallFloat());
            stream.WriteVector3(value);

            Assert.AreEqual(size, stream.BitOffset);
            stream.ResetRead();

            Assert.AreEqual(value, stream.ReadVector3());
            Assert.AreEqual(size, stream.BitOffset);
        }

        [Test]
        public void Vector4Test()
        {
            int size = sizeof(float) * 8 * 4;
            Vector4 value = new Vector4(rnd.NextSmallFloat(), rnd.NextSmallFloat(), rnd.NextSmallFloat(), rnd.NextSmallFloat());
            stream.WriteVector4(value);

            Assert.AreEqual(size, stream.BitOffset);
            stream.ResetRead();

            Assert.AreEqual(value, stream.ReadVector4());
            Assert.AreEqual(size, stream.BitOffset);
        }

        [Test]
        public void QuaternionTest()
        {
            int size = sizeof(float) * 8 * 4;
            Quaternion value = new Quaternion(rnd.NextSmallFloat(), rnd.NextSmallFloat(), rnd.NextSmallFloat(), rnd.NextSmallFloat());
            stream.WriteQuaternion(value);

            Assert.AreEqual(size, stream.BitOffset);
            stream.ResetRead();

            Assert.AreEqual(value, stream.ReadQuaternion());
            Assert.AreEqual(size, stream.BitOffset);
        }

        [Test]
        public void Vector2IntTest()
        {
            Vector2Int value = new Vector2Int(rnd.Next(), rnd.Next());
            stream.Skip(1);
            stream.WriteVector2Int(value);

            Assert.AreEqual(65, stream.BitOffset);
            stream.ResetRead();
            stream.Skip(1);

            Assert.AreEqual(value, stream.ReadVector2Int());
            Assert.AreEqual(65, stream.BitOffset);
        }

        [Test]
        public void Vector3IntTest()
        {
            int size = sizeof(int) * 8 * 3;
            Vector3Int value = new Vector3Int(rnd.Next(), rnd.Next(), rnd.Next());
            stream.WriteVector3Int(value);

            Assert.AreEqual(size, stream.BitOffset);
            stream.ResetRead();

            Assert.AreEqual(value, stream.ReadVector3Int());
            Assert.AreEqual(size, stream.BitOffset);
        }


        [Test]
        public void Vector2HalfTest()
        {
            Vector2 value = new Vector2((float)Math.PI, 48.13f);
            stream.Skip(1);
            stream.WriteVector2Half(value);

            Assert.AreEqual(33, stream.BitOffset);
            stream.ResetRead();
            stream.Skip(1);

            AreSimilar(value, stream.ReadVector2Half(), 0.01f);
            Assert.AreEqual(33, stream.BitOffset);
        }

        [Test]
        public void Vector3HalfTest()
        {
            int size = sizeof(float) * 4 * 3;
            Vector3 value = new Vector3(rnd.NextSmallFloat(), rnd.NextSmallFloat(), rnd.NextSmallFloat());
            stream.WriteVector3Half(value);

            Assert.AreEqual(size, stream.BitOffset);
            stream.ResetRead();

            AreSimilar(value, stream.ReadVector3Half(), 0.001f);
            Assert.AreEqual(size, stream.BitOffset);
        }

        [Test]
        public void Vector4HalfTest()
        {
            int size = sizeof(float) * 4 * 4;
            Vector4 value = new Vector4(rnd.NextSmallFloat(), rnd.NextSmallFloat(), rnd.NextSmallFloat(), rnd.NextSmallFloat());
            stream.WriteVector4Half(value);

            Assert.AreEqual(size, stream.BitOffset);
            stream.ResetRead();

            AreSimilar(value, stream.ReadVector4Half(), 0.001f);
            Assert.AreEqual(size, stream.BitOffset);
        }

        [Test]
        public void QuaternionHalfTest()
        {
            int size = sizeof(float) * 4 * 4;
            Quaternion value = new Quaternion(rnd.NextSmallFloat(), rnd.NextSmallFloat(), rnd.NextSmallFloat(), rnd.NextSmallFloat());
            stream.WriteQuaternionHalf(value);

            Assert.AreEqual(size, stream.BitOffset);
            stream.ResetRead();

            AreSimilar(value, stream.ReadQuaternionHalf(), 0.001f);
            Assert.AreEqual(size, stream.BitOffset);
        }


        [Test]
        public void Vector2IntBitTest()
        {
            int size = 8 * 2 + 1;
            Vector2Int value = new Vector2Int(127, -123);
            stream.Skip(1);
            stream.WriteVector2Int(value, 8);

            Assert.AreEqual(size, stream.BitOffset);
            stream.ResetRead();
            stream.Skip(1);

            Assert.AreEqual(value, stream.ReadVector2Int(8));
            Assert.AreEqual(size, stream.BitOffset);
        }

        [Test]
        public void Vector2IntMinMaxTest()
        {
            int size = 8 * 2 + 1;
            Vector2Int value = new Vector2Int(127, 250);
            stream.Skip(1);
            stream.WriteVector2Int(value, 0, 255);

            Assert.AreEqual(size, stream.BitOffset);
            stream.ResetRead();
            stream.Skip(1);

            Assert.AreEqual(value, stream.ReadVector2Int(0, 255));
            Assert.AreEqual(size, stream.BitOffset);
        }

        [Test]
        public void SerializeVector2Test()
        {
            Vector2 value = new Vector2(rnd.NextFloat(), rnd.NextFloat());
            Vector2 replica = new Vector2();

            stream.Skip(1);
            stream.Serialize(ref value);

            Assert.AreEqual(65, stream.BitOffset);
            stream.ResetRead();
            stream.Skip(1);

            stream.Serialize(ref replica);

            Assert.AreEqual(value, replica);
            Assert.AreEqual(65, stream.BitOffset);
        }

        [Test]
        public void SerializeVector2HalfTest()
        {
            Vector2 value = new Vector2(rnd.NextSmallFloat(), rnd.NextSmallFloat());
            Vector2 replica = new Vector2();

            stream.Serialize(ref value, true);

            Assert.AreEqual(32, stream.BitOffset);
            stream.ResetRead();

            stream.Serialize(ref replica, true);

            Assert.AreEqual(value, replica);
            Assert.AreEqual(32, stream.BitOffset);
        }

        [Test]
        public void SerializeVector2MinMaxTest()
        {
            Vector2 value = new Vector2(1.581f, -53.123f);
            Vector2 replica = new Vector2();

            stream.Serialize(ref value, -100f, 10f, 0.001f);

            stream.ResetRead();

            stream.Serialize(ref replica, -100f, 10f, 0.001f);

            AreSimilar(value, replica, 0.001f);
        }

        [Test]
        public void SerializeVector2IntTest()
        {
            Vector2Int value = new Vector2Int(rnd.Next(), rnd.Next());
            Vector2Int replica = new Vector2Int();

            stream.Serialize(ref value);

            Assert.AreEqual(64, stream.BitOffset);
            stream.ResetRead();

            stream.Serialize(ref replica);

            Assert.AreEqual(value, replica);
            Assert.AreEqual(64, stream.BitOffset);
        }

        [Test]
        public void SerializeVector2IntMinMaxTest()
        {
            int bits = MathUtils.BitsRequired(-250, 250) * 2;

            Vector2Int value = new Vector2Int(-42, 183);
            Vector2Int replica = new Vector2Int();

            stream.Serialize(ref value, -250, 250);

            Assert.AreEqual(bits, stream.BitOffset);
            stream.ResetRead();

            stream.Serialize(ref replica, -250, 250);

            Assert.AreEqual(value, replica);
            Assert.AreEqual(bits, stream.BitOffset);
        }


        private static void AreSimilar(Vector2 a, Vector2 b, float delta)
        {
            Assert.AreEqual(a.x, b.x, delta);
            Assert.AreEqual(a.y, b.y, delta);
        }

        private static void AreSimilar(Vector3 a, Vector3 b, float delta)
        {
            Assert.AreEqual(a.x, b.x, delta);
            Assert.AreEqual(a.y, b.y, delta);
            Assert.AreEqual(a.z, b.z, delta);
        }

        private static void AreSimilar(Vector4 a, Vector4 b, float delta)
        {
            Assert.AreEqual(a.x, b.x, delta);
            Assert.AreEqual(a.y, b.y, delta);
            Assert.AreEqual(a.z, b.z, delta);
            Assert.AreEqual(a.w, b.w, delta);
        }

        private static void AreSimilar(Quaternion a, Quaternion b, float delta)
        {
            Assert.AreEqual(a.x, b.x, delta);
            Assert.AreEqual(a.y, b.y, delta);
            Assert.AreEqual(a.z, b.z, delta);
            Assert.AreEqual(a.w, b.w, delta);
        }
    }

    internal static unsafe class RandomExtension
    {
        public static float NextSmallFloat(this System.Random rnd)
        {
            var i = (ushort)rnd.Next();
            return HalfPrecision.Decompress(i);
        }

        public static float NextFloat(this System.Random rnd)
        {
            var i = rnd.Next();
            return *(float*)&i;
        }
    }
}
