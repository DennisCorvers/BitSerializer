using System.Runtime.CompilerServices;

namespace BitSerializer
{
    public unsafe partial class BitStreamer
    {
        /// <summary>
        /// Only use this with types that have a strict layout!
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public BitStreamer ReadBlit<T>(T* value)
            where T : unmanaged
        {
            ReadMemory(value, sizeof(T));
            return this;
        }

        /// <summary>
        /// Only use this with types that have a strict layout!
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T ReadBlit<T>()
            where T : unmanaged
        {
            T* value = stackalloc T[1];
            ReadMemory(value, sizeof(T));
            return *value;
        }

        /// <summary>
        /// Only use this with types that have a strict layout!
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public BitStreamer WriteBlit<T>(T value)
            where T : unmanaged
        {
            WriteMemory(&value, sizeof(T));
            return this;
        }

        /// <summary>
        /// Only use this with types that have a strict layout!
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public BitStreamer WriteBlit<T>(T* value)
            where T : unmanaged
        {
            WriteMemory(value, sizeof(T));
            return this;
        }

        /// <summary>
        /// Only use this with types that have a strict layout!
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SerializeBlit<T>(ref T value)
            where T : unmanaged
        {
            if (m_mode == SerializationMode.Writing) WriteBlit(value);
            else value = ReadBlit<T>();
        }

        /// <summary>
        /// Only use this with types that have a strict layout!
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public BitStreamer SerializeBlit<T>(T* value)
            where T : unmanaged
        {
            if (m_mode == SerializationMode.Writing) WriteBlit(value);
            else ReadBlit(value);
            return this;
        }
    }
}
