using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
#if UNITY
using Unity.Collections.LowLevel.Unsafe;
#endif

namespace BitSerializer.Utils
{
    internal unsafe static class Memory
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static IntPtr Alloc(int size)
        {
#if UNITY
            return (IntPtr)UnsafeUtility.Malloc(size, 8, Unity.Collections.Allocator.Persistent);
#else
            return Marshal.AllocHGlobal(size);
#endif
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Dealloc(IntPtr ptr)
        {
#if UNITY
            UnsafeUtility.Free((void*)ptr, Unity.Collections.Allocator.Persistent);
#else
            Marshal.FreeHGlobal(ptr);
#endif
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Dealloc(void* ptr)
        {
#if UNITY
            UnsafeUtility.Free(ptr, Unity.Collections.Allocator.Persistent);
#else
            Marshal.FreeHGlobal((IntPtr)ptr);
#endif
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static IntPtr Realloc(IntPtr ptr, int size, int newSize)
        {
            Debug.Assert(newSize > size);
#if UNITY
            IntPtr newBuffer = Alloc(newSize);
            CopyMemory(ptr, 0, newBuffer, 0, size);

            // Free the old buffer and return the new one.
            Dealloc(ptr);
            return newBuffer;
#else
            return Marshal.ReAllocHGlobal(ptr, (IntPtr)size);
#endif
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe static void CopyMemory(void* source, int sourceIndex, void* destination, int destinationIndex, int length)
        {
            Debug.Assert(destinationIndex >= 0);
            Debug.Assert(sourceIndex >= 0);
            Debug.Assert(length >= 0);

#if UNITY
            UnsafeUtility.MemCpy((byte*)destination + destinationIndex, (byte*)source + sourceIndex, length);
#else
            Buffer.MemoryCopy(
                (byte*)source + sourceIndex,
                (byte*)destination + destinationIndex,
                length, length);
#endif
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe static void CopyMemory(IntPtr source, int sourceIndex, IntPtr destination, int destinationIndex, int length)
        {
            CopyMemory((void*)source, sourceIndex, (void*)destination, destinationIndex, length);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe static void CopyMemory(byte[] source, int sourceIndex, IntPtr destination, int destinationIndex, int length)
        {
            if (source.Length < sourceIndex + length)
                throw new ArgumentOutOfRangeException("Offset exceeds buffer size.");
            if (sourceIndex < 0 || length < 0)
                throw new ArgumentOutOfRangeException("Index and Length must be greater than 0");
#if UNITY
            fixed (byte* ptr = source)
            {
                CopyMemory(ptr, sourceIndex, (void*)destination, destinationIndex, length);
            }
#else
            Marshal.Copy(source, sourceIndex, destination + destinationIndex, length);
#endif

        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe static void CopyMemory(IntPtr source, int sourceIndex, byte[] destination, int destinationIndex, int length)
        {
            if (destination.Length < destinationIndex + length)
                throw new ArgumentOutOfRangeException("Offset exceeds buffer size.");
            if (destinationIndex < 0 || length < 0)
                throw new ArgumentOutOfRangeException("Index and Length must be greater than 0");

#if UNITY
            fixed (byte* ptr = &destination[destinationIndex])
            {
                CopyMemory((void*)source, sourceIndex, ptr, destinationIndex, length);
            }
#else
            Marshal.Copy(source + sourceIndex, destination, destinationIndex, length);
#endif
        }
    }
}
