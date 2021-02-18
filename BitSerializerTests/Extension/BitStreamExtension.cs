using BitSerializer;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace BitSerializerTests.Extension
{
    public static class BitStreamExtension
    {
        public static void WriteVector3(this BitStreamer stream, Vector3 value)
        {
            stream.WriteHalf(value.X);
            stream.WriteHalf(value.Y);
            stream.WriteHalf(value.Z);
        }

        public static Vector3 ReadVector3(this BitStreamer stream)
        {
            return new Vector3(
                stream.ReadHalf(),
                stream.ReadHalf(),
                stream.ReadHalf()
                );
        }

        public static void Serialize(this BitStreamer stream, ref Vector3 value)
        {
            if (stream.IsWriting) WriteVector3(stream, value);
            else value = ReadVector3(stream);
        }
    }
}
