using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace BitSerializer.Types
{
    [Serializable]
    [StructLayout(LayoutKind.Explicit, Size = 1)]
    public struct BlittableBool : IEquatable<BlittableBool>, IComparable<BlittableBool>
    {
        [FieldOffset(0)]
        private byte m_value;

        public int CompareTo(BlittableBool other)
        {
            if (other.m_value == m_value) { return 0; }
            return m_value == 1 ? 1 : -1;
        }
        public bool Equals(BlittableBool other)
        {
            return m_value == other.m_value;
        }

        public override string ToString()
        {
            return (m_value == 1).ToString();
        }

        public static implicit operator bool(BlittableBool val)
        {
            return val.m_value == 1;
        }
        public static implicit operator BlittableBool(bool val)
        {
            return new BlittableBool() { m_value = (byte)(val ? 1 : 0) };
        }
        public static implicit operator byte(BlittableBool val)
        {
            return val.m_value;
        }
        public static implicit operator BlittableBool(byte val)
        {
            return new BlittableBool() { m_value = (byte)(val > 0 ? 1 : 0) };
        }
    }
}
