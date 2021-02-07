using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace BitSerializer.Utils
{
    internal unsafe static class Memory
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static IntPtr Alloc(int size)
        {
            return Marshal.AllocHGlobal(size);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Dealloc(IntPtr ptr)
        {
            Marshal.FreeHGlobal(ptr);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Dealloc(void* ptr)
        {
            Marshal.FreeHGlobal((IntPtr)ptr);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static IntPtr Realloc(IntPtr ptr, int size)
        {
            return Marshal.ReAllocHGlobal(ptr, (IntPtr)size);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe static void CopyMemory(void* source, int sourceIndex, void* destination, int destinationIndex, int length)
        {
            if (destinationIndex < 0 || length < 0)
            { throw new ArgumentOutOfRangeException("Index and Length must be greater than 0"); }

            Buffer.MemoryCopy(
                (byte*)source + sourceIndex,
                (byte*)destination + destinationIndex,
                length, length);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe static void CopyMemory(IntPtr source, int sourceIndex, IntPtr destination, int destinationIndex, int length)
        {
            if (destinationIndex < 0 || length < 0)
            { throw new ArgumentOutOfRangeException("Index and Length must be greater than 0"); }

            Buffer.MemoryCopy(((byte*)source) + sourceIndex, ((byte*)destination) + destinationIndex, length, length);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe static void CopyMemory(byte[] source, int sourceIndex, IntPtr destination, int destinationIndex, int length)
        {
            if (source.Length < sourceIndex + length)
            { throw new ArgumentOutOfRangeException("Offset exceeds buffer size."); }
            if (sourceIndex < 0 || length < 0)
            { throw new ArgumentOutOfRangeException("Index and Length must be greater than 0"); }

            Marshal.Copy(source, sourceIndex, destination + destinationIndex, length);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe static void CopyMemory(IntPtr source, int sourceIndex, byte[] destination, int destinationIndex, int length)
        {
            if (destination.Length < destinationIndex + length)
            { throw new ArgumentOutOfRangeException("Offset exceeds buffer size."); }
            if (destinationIndex < 0 || length < 0)
            { throw new ArgumentOutOfRangeException("Index and Length must be greater than 0"); }

            Marshal.Copy(source + sourceIndex, destination, destinationIndex, length);
        }
    }
}
