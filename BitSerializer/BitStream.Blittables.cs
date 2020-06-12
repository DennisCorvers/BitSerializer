using System.Runtime.CompilerServices;

namespace BitSerializer
{
    public unsafe partial class BitStream
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public BitStream WriteBlit<T>(T value)
            where T : unmanaged
        {
            WriteMemory(&value, sizeof(T));
            return this;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public BitStream WriteBlit<T>(T* value)
            where T : unmanaged
        {
            WriteMemory(value, sizeof(T));
            return this;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public BitStream ReadBlit<T>(T* value)
            where T : unmanaged
        {
            ReadMemory(value, sizeof(T));
            return this;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T ReadBlit<T>()
            where T : unmanaged
        {
            T* value = stackalloc T[1];
            ReadMemory(value, sizeof(T));
            return *value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SerializeBlit<T>(ref T value)
            where T : unmanaged
        {
            if (m_mode == SerializationMode.Writing) WriteBlit(value);
            else value = ReadBlit<T>();
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public BitStream SerializeBlit<T>(T* value)
            where T : unmanaged
        {
            if (m_mode == SerializationMode.Writing) WriteBlit(value);
            else ReadBlit(value);
            return this;
        }
    }
}
