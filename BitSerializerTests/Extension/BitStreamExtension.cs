using BitSerializer;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace BitSerializerTests.Extension
{
    public static class BitStreamExtension
    {
        public static void WriteVector3(this BitStream stream, Vector3 value)
        {
            stream.WriteHalf(value.X);
            stream.WriteHalf(value.Y);
            stream.WriteHalf(value.Z);
        }

        public static Vector3 ReadVector3(this BitStream stream)
        {
            return new Vector3(
                stream.ReadHalf(), 
                stream.ReadHalf(), 
                stream.ReadHalf()
                );
        }

        public static void Serialize(this BitStream stream, ref Vector3 value)
        {
            if (stream.IsWriting) stream.WriteVector3(value);
            else value = stream.ReadVector3();
        }
    }
}
